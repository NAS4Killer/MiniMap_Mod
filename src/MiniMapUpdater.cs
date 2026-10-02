using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

internal static class MiniMapUpdater
{
    private const string Repository = "https://github.com/NAS4Killer/MiniMap_Mod";
    private const int MaxPackageSize = 20 * 1024 * 1024;
    private static readonly object Gate = new object();
    private static bool busy, pending;
    private static Release available;
    private static volatile string status = "Noch nicht geprüft";
    public static string Status => status;
    public static string Caption { get { lock (Gate) return pending ? "BEENDEN ZUM UPDATE" : busy ? "BITTE WARTEN" : available != null ? "UPDATE INSTALLIEREN" : "UPDATE PRÜFEN"; } }

    [DataContract] internal sealed class Release
    {
        [DataMember] public string tag_name;
        [DataMember] public bool draft;
        [DataMember] public bool prerelease;
        [DataMember] public Asset[] assets;
    }
    [DataContract] internal sealed class Asset
    {
        [DataMember] public string name;
        [DataMember] public string browser_download_url;
        [DataMember] public string digest;
        [DataMember] public long size;
    }

    internal static Version ParseVersion(string value)
    {
        if (value == null) throw new InvalidDataException("Version fehlt.");
        string text = value.Trim().TrimStart('v', 'V');
        if (text.Split('.').Length != 4 || !Version.TryParse(text, out Version version))
            throw new InvalidDataException("Ungültige Versionsnummer.");
        return version;
    }

    public static void Press()
    {
        lock (Gate)
        {
            if (busy || pending) return;
            busy = true;
        }
        Task.Run(async () =>
        {
            try
            {
                Release release;
                lock (Gate) release = available;
                if (release == null) await Check();
                else await PrepareInstallation(release);
            }
            catch (Exception error)
            {
                status = "Update fehlgeschlagen; erneut prüfen";
                // No tokens or personal paths in the in-game status.
                Trace.WriteLine("[MiniMap_Mod] Update: " + error.GetType().Name);
                lock (Gate) available = null;
            }
            finally { lock (Gate) busy = false; }
        });
    }

    private static HttpClient Client()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("MiniMap_Mod/" + MiniMapPreferences.Version);
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return client;
    }

    private static async Task Check()
    {
        status = "GitHub wird geprüft ...";
        using (var client = Client())
        using (var response = await client.GetAsync("https://api.github.com/repos/NAS4Killer/MiniMap_Mod/releases/latest"))
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) { status = "Noch kein Release verfügbar"; return; }
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || (int)response.StatusCode == 429)
            { status = "GitHub-Limit; später erneut prüfen"; return; }
            response.EnsureSuccessStatusCode();
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var release = (Release)new DataContractJsonSerializer(typeof(Release)).ReadObject(stream);
                Version remote = ParseVersion(release.tag_name);
                if (release.draft || release.prerelease) throw new InvalidDataException("Kein stabiles Release.");
                if (remote <= ParseVersion(MiniMapPreferences.Version))
                { status = "Aktuell (GitHub " + remote + ")"; return; }
                GetAsset(release);
                lock (Gate) available = release;
                status = "Neue Version " + remote + " verfügbar";
            }
        }
    }

    private static Asset GetAsset(Release release)
    {
        string version = ParseVersion(release.tag_name).ToString();
        if (release.assets != null) foreach (Asset asset in release.assets)
        {
            if (asset.name != "MiniMap_Mod-v" + version + ".zip") continue;
            if (asset.size <= 0 || asset.size > MaxPackageSize || asset.digest == null ||
                !System.Text.RegularExpressions.Regex.IsMatch(asset.digest, "^sha256:[a-fA-F0-9]{64}$"))
                throw new InvalidDataException("ZIP-Prüfsumme fehlt oder Paket zu groß.");
            string expected = Repository + "/releases/download/" + release.tag_name + "/" + asset.name;
            if (asset.browser_download_url != expected) throw new InvalidDataException("Unerwartete Downloadadresse.");
            return asset;
        }
        throw new InvalidDataException("Release-ZIP fehlt.");
    }

    internal static void ExtractPackage(byte[] bytes, string directory, string version)
    {
        var allowed = new HashSet<string>(StringComparer.Ordinal) {
            "ModInfo.xml", "MiniMap_Mod.dll", "Config/XUi_InGame/windows.xml",
            "Config/XUi_InGame/xui.xml", "VERSION", "README.md", "ReleaseNotes-" + version + ".txt"
        };
        var found = new HashSet<string>(StringComparer.Ordinal);
        using (var memory = new MemoryStream(bytes))
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Read))
        {
            long total = 0;
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                string name = entry.FullName;
                if (name.EndsWith("/", StringComparison.Ordinal))
                {
                    if (name != "MiniMap_Mod/" && name != "MiniMap_Mod/Config/" && name != "MiniMap_Mod/Config/XUi_InGame/")
                        throw new InvalidDataException("Unerwartetes ZIP-Verzeichnis.");
                    continue;
                }
                if (!name.StartsWith("MiniMap_Mod/", StringComparison.Ordinal)) throw new InvalidDataException("Ungültige ZIP-Struktur.");
                string relative = name.Substring("MiniMap_Mod/".Length);
                if (!allowed.Contains(relative) || !found.Add(relative)) throw new InvalidDataException("Unerwartete oder doppelte ZIP-Datei.");
                total += entry.Length;
                if (entry.Length <= 0 || total > MaxPackageSize) throw new InvalidDataException("Ungültige Paketgröße.");
            }
            foreach (string required in allowed) if (!found.Contains(required)) throw new InvalidDataException("Paketdatei fehlt: " + required);
            // Validate metadata before extracting anything executable.
            XDocument info;
            using (var metadata = zip.GetEntry("MiniMap_Mod/ModInfo.xml").Open()) info = XDocument.Load(metadata);
            if ((string)info.Root?.Element("Name")?.Attribute("value") != "MiniMap_Mod" ||
                (string)info.Root?.Element("Author")?.Attribute("value") != "NAS4Killer" ||
                (string)info.Root?.Element("Version")?.Attribute("value") != version)
                throw new InvalidDataException("Mod-Metadaten passen nicht zum Release.");
            using (var reader = new StreamReader(zip.GetEntry("MiniMap_Mod/VERSION").Open()))
                if (reader.ReadToEnd().Trim() != version) throw new InvalidDataException("Paketversion stimmt nicht.");
            foreach (string relative in allowed)
            {
                string target = Path.Combine(directory, relative.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                using (var input = zip.GetEntry("MiniMap_Mod/" + relative).Open())
                using (var output = File.Create(target)) input.CopyTo(output);
            }
        }
    }

    private static async Task PrepareInstallation(Release release)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT) throw new PlatformNotSupportedException();
        Asset asset = GetAsset(release);
        status = "ZIP wird geladen und geprüft ...";
        byte[] bytes;
        using (var client = Client())
        using (var response = await client.GetAsync(asset.browser_download_url, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();
            using (var input = await response.Content.ReadAsStreamAsync())
            using (var memory = new MemoryStream())
            {
                var buffer = new byte[8192];
                int count;
                while ((count = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    if (memory.Length + count > MaxPackageSize) throw new InvalidDataException("ZIP zu groß.");
                    memory.Write(buffer, 0, count);
                }
                bytes = memory.ToArray();
            }
        }
        using (var sha = SHA256.Create())
        {
            string hash = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
            if (bytes.LongLength != asset.size || asset.digest != "sha256:" + hash) throw new InvalidDataException("ZIP-Prüfsumme falsch.");
        }
        string version = ParseVersion(release.tag_name).ToString();
        string job = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "7DaysToDie", "MiniMap_Updates", Guid.NewGuid().ToString("N"));
        string payload = Path.Combine(job, "payload");
        ExtractPackage(bytes, payload, version);
        string installer = Path.Combine(job, "Install.ps1");
        using (Stream resource = typeof(MiniMapUpdater).Assembly.GetManifestResourceStream("MiniMapMod.UpdateInstaller.ps1"))
        using (Stream output = File.Create(installer)) resource.CopyTo(output);
        string target = Path.GetDirectoryName(typeof(MiniMapUpdater).Assembly.Location);
        string command = "& " + Quote(installer) + " -Target " + Quote(target) + " -Payload " + Quote(payload) +
            " -GameProcessId " + Process.GetCurrentProcess().Id + " -ExpectedVersion " + Quote(version);
        var process = Process.Start(new ProcessStartInfo {
            FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe"),
            Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand " + Convert.ToBase64String(Encoding.Unicode.GetBytes(command)),
            UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden
        });
        if (process == null) throw new IOException("Installer konnte nicht gestartet werden.");
        process.Dispose();
        lock (Gate) pending = true;
        status = "Download bereit. Spiel beenden.";
    }

    private static string Quote(string value) => "'" + value.Replace("'", "''") + "'";
}
