using zipfile;
using configparser;
using BytesIO = io.BytesIO;
using isfile = os.path.isfile;
using join = os.path.join;
using listdir = os.listdir;
using shutil;
using tempfile;
using SiteHandler;
using requests = packages.requests;
using System.Collections.Generic;

public static class WoWAddonUpdater {
    
    public static object confirmExit() {
        input("\nPress the Enter key to exit");
        exit(0);
    }
    
    public class AddonUpdater {
        
        public AddonUpdater() {
            Console.WriteLine("");
            // Read config file
            if (!isfile("config.ini")) {
                Console.WriteLine("Failed to read configuration file. Are you sure there is a file called \"config.ini\"?\n");
                confirmExit();
            }
            var config = configparser.ConfigParser();
            config.read("config.ini");
            try {
                this.WOW_ADDON_LOCATION = config["WOW ADDON UPDATER"]["WoW Addon Location"];
                this.ADDON_LIST_FILE = config["WOW ADDON UPDATER"]["Addon List File"];
                this.INSTALLED_VERS_FILE = config["WOW ADDON UPDATER"]["Installed Versions File"];
                this.AUTO_CLOSE = config["WOW ADDON UPDATER"]["Close Automatically When Completed"];
            } catch (Exception) {
                Console.WriteLine("Failed to parse configuration file. Are you sure it is formatted correctly?\n");
                confirmExit();
            }
            if (!isfile(this.ADDON_LIST_FILE)) {
                Console.WriteLine("Failed to read addon list file. Are you sure the file exists?\n");
                confirmExit();
            }
            if (!isfile(this.INSTALLED_VERS_FILE)) {
                using (var newInstalledVersFile = open(this.INSTALLED_VERS_FILE, "w")) {
                    newInstalledVers = configparser.ConfigParser();
                    newInstalledVers["Installed Versions"] = new Dictionary<object, object> {
                    };
                    newInstalledVers.write(newInstalledVersFile);
                }
            }
            return;
        }
        
        public virtual object update() {
            var uberlist = new List<object>();
            using (var fin = open(this.ADDON_LIST_FILE, "r")) {
                foreach (var line in fin) {
                    current_node = new List<object>();
                    line = line.rstrip("\n");
                    if (!line || line.startswith("#")) {
                        continue;
                    }
                    if (line.Contains("|")) {
                        // Expected input format: "mydomain.com/myzip.zip" or "mydomain.com/myzip.zip|subfolder"
                        subfolder = line.split("|")[1];
                        line = line.split("|")[0];
                    } else {
                        subfolder = "";
                    }
                    addonName = SiteHandler.getAddonName(line);
                    currentVersion = SiteHandler.getCurrentVersion(line);
                    if (currentVersion == null) {
                        currentVersion = "Not Available";
                    }
                    current_node.append(addonName);
                    current_node.append(currentVersion);
                    installedVersion = this.getInstalledVersion(line, subfolder);
                    if (!(currentVersion == installedVersion)) {
                        Console.WriteLine("Installing/updating addon: " + addonName + " to version: " + currentVersion + "\n");
                        ziploc = SiteHandler.findZiploc(line);
                        install_success = false;
                        install_success = this.getAddon(ziploc, subfolder);
                        current_node.append(this.getInstalledVersion(line, subfolder));
                        if (install_success && currentVersion != "") {
                            this.setInstalledVersion(line, subfolder, currentVersion);
                        }
                    } else {
                        Console.WriteLine(addonName + " version " + currentVersion + " is up to date.\n");
                        current_node.append("Up to date");
                    }
                    uberlist.append(current_node);
                }
            }
            if (this.AUTO_CLOSE == "False") {
                var col_width = max(from row in uberlist
                    from word in row
                    select word.Count) + 2;
                Console.WriteLine("".join(from word in Tuple.Create("Name", "Iversion", "Cversion")
                    select word.ljust(col_width)));
                foreach (var row in uberlist) {
                    Console.WriteLine("".join(from word in row
                        select word.ljust(col_width)), end: "\n");
                }
                confirmExit();
            }
        }
        
        public virtual object getAddon(object ziploc, object subfolder) {
            if (ziploc == "") {
                return false;
            }
            try {
                var r = requests.get(ziploc, stream: true);
                r.raise_for_status();
                var z = zipfile.ZipFile(BytesIO(r.content));
                this.extract(z, ziploc, subfolder);
                return true;
            } catch (Exception) {
                Console.WriteLine("Failed to download or extract zip file for addon. Skipping...\n");
                return false;
            }
        }
        
        public virtual object extract(object zip, object url, object subfolder) {
            if (subfolder == "") {
                zip.extractall(this.WOW_ADDON_LOCATION);
            } else {
                // Pull subfolder out to main level, remove original extracted folder
                try {
                    using (var tempDirPath = tempfile.TemporaryDirectory()) {
                        zip.extractall(tempDirPath);
                        extractedFolderPath = join(tempDirPath, listdir(tempDirPath)[0]);
                        subfolderPath = join(extractedFolderPath, subfolder);
                        destination_dir = join(this.WOW_ADDON_LOCATION, subfolder);
                        // Delete the existing copy, as shutil.copytree will not work if
                        // the destination directory already exists!
                        shutil.rmtree(destination_dir, ignore_errors: true);
                        shutil.copytree(subfolderPath, destination_dir);
                    }
                } catch (Exception) {
                    Console.WriteLine("Failed to get subfolder " + subfolder);
                }
            }
        }
        
        public virtual object getInstalledVersion(object addonpage, object subfolder) {
            var addonName = SiteHandler.getAddonName(addonpage);
            var installedVers = configparser.ConfigParser();
            installedVers.read(this.INSTALLED_VERS_FILE);
            try {
                if (subfolder) {
                    return installedVers["Installed Versions"][addonName + "|" + subfolder];
                } else {
                    return installedVers["Installed Versions"][addonName];
                }
            } catch (Exception) {
                return "version not found";
            }
        }
        
        public virtual object setInstalledVersion(object addonpage, object subfolder, object currentVersion) {
            var addonName = SiteHandler.getAddonName(addonpage);
            var installedVers = configparser.ConfigParser();
            installedVers.read(this.INSTALLED_VERS_FILE);
            if (subfolder) {
                installedVers.set("Installed Versions", addonName + "|" + subfolder, currentVersion);
            } else {
                installedVers.set("Installed Versions", addonName, currentVersion);
            }
            using (var installedVersFile = open(this.INSTALLED_VERS_FILE, "w")) {
                installedVers.write(installedVersFile);
            }
        }
    }
    
    public static object main() {
        if (isfile("changelog.txt")) {
            var downloadedChangelog = requests.get("https://raw.githubusercontent.com/kuhnerdm/wow-addon-updater/master/changelog.txt").text.split("\n");
            using (var cl = open("changelog.txt")) {
                presentChangelog = cl.readlines();
                foreach (var i in range(presentChangelog.Count)) {
                    presentChangelog[i] = presentChangelog[i].strip("\n");
                }
            }
        }
        if (downloadedChangelog != presentChangelog) {
            Console.WriteLine("A new update to WoWAddonUpdater is available! Check it out at https://github.com/kuhnerdm/wow-addon-updater !");
        }
        var addonupdater = AddonUpdater();
        addonupdater.update();
        return;
    }
    
    static WoWAddonUpdater() {
        main();
    }
}
