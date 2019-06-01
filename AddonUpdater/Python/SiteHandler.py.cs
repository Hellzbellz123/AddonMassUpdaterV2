public static class SiteHandler
{
    // Site splitter
    public static object findZiploc(object addonpage)
    {
        // Curse
        if (addonpage.startswith("https://mods.curse.com/addons/wow/"))
        {
            return curse(convertOldCurseURL(addonpage));
        }
        else if (addonpage.startswith("https://www.curseforge.com/wow/addons/"))
        {
            return curse(addonpage);
        }
        else if (addonpage.startswith("https://wow.curseforge.com/projects/"))
        {
            // Curse Project
            if (addonpage.endswith("/files"))
            {
                // Remove /files from the end of the URL, since it gets added later
                return curseProject.Remove(addonpage.Length - 6);
            }
            else
            {
                return curseProject(addonpage);
            }
        }
        else if (addonpage.startswith("https://www.wowace.com/projects/"))
        {
            // WowAce Project
            if (addonpage.endswith("/files"))
            {
                // Remove /files from the end of the URL, since it gets added later
                return wowAceProject.Remove(addonpage.Length - 6);
            }
            else
            {
                return wowAceProject(addonpage);
            }
        }
        else if (addonpage.startswith("https://git.tukui.org/"))
        {
            // Tukui
            return tukui(addonpage);
        }
        else if (addonpage.startswith("http://www.wowinterface.com/"))
        {
            // Wowinterface
            return wowinterface(addonpage);
        }
        else
        {
            // Invalid page
            Console.WriteLine("Invalid addon page.");
        }
    }

    public static object getCurrentVersion(object addonpage)
    {
        // Curse
        if (addonpage.startswith("https://mods.curse.com/addons/wow/"))
        {
            return getCurseVersion(convertOldCurseURL(addonpage));
        }
        else if (addonpage.startswith("https://www.curseforge.com/wow/addons/"))
        {
            return getCurseVersion(addonpage);
        }
        else if (addonpage.startswith("https://wow.curseforge.com/projects/"))
        {
            // Curse Project
            return getCurseProjectVersion(addonpage);
        }
        else if (addonpage.startswith("https://www.wowace.com/projects/"))
        {
            // WowAce Project
            return getWowAceProjectVersion(addonpage);
        }
        else if (addonpage.startswith("https://git.tukui.org/"))
        {
            // Tukui
            return getTukuiVersion(addonpage);
        }
        else if (addonpage.startswith("http://www.wowinterface.com/"))
        {
            // Wowinterface
            return getWowinterfaceVersion(addonpage);
        }
        else
        {
            // Invalid page
            Console.WriteLine("Invalid addon page.");
        }
    }

    public static object getAddonName(object addonpage)
    {
        var addonName = addonpage.replace("https://mods.curse.com/addons/wow/", "");
        addonName = addonName.replace("https://www.curseforge.com/wow/addons/", "");
        addonName = addonName.replace("https://wow.curseforge.com/projects/", "");
        addonName = addonName.replace("http://www.wowinterface.com/downloads/", "");
        addonName = addonName.replace("https://www.wowace.com/projects/", "");
        addonName = addonName.replace("https://git.tukui.org/", "");

        return addonName;
    }

    // Curse
    public static object curse(object addonpage)
    {
        if (addonpage.Contains("/datastore"))
        {
            return curseDatastore(addonpage);
        }
        try
        {
            var page = requests.get(addonpage + "/download");
            page.raise_for_status();
            var contentString = str(page.content);
            var indexOfZiploc = contentString.find("download__link") + 22;
            var endQuote = contentString.find("\"", indexOfZiploc);
            return "https://www.curseforge.com" + contentString[indexOfZiploc::endQuote];
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to find downloadable zip file for addon. Skipping...\n");
            return "";
        }
    }

    public static object curseDatastore(object addonpage)
    {
        try
        {
            // First, look for the URL of the project file page
            var page = requests.get(addonpage);
            page.raise_for_status();
            var contentString = str(page.content);
            var endOfProjectPageURL = contentString.find("\">Visit Project Page");
            var indexOfProjectPageURL = contentString.rfind("<a href=\"", 0, endOfProjectPageURL) + 9;
            var projectPage = contentString[indexOfProjectPageURL::endOfProjectPageURL] + "/files";
            // Then get the project page and get the URL of the first (most recent) file
            page = requests.get(projectPage);
            page.raise_for_status();
            projectPage = page.url;
            contentString = str(page.content);
            var startOfTable = contentString.find("project-file-name-container");
            var indexOfZiploc = contentString.find("<a class=\"button tip fa-icon-download icon-only\" href=\"/", startOfTable) + 55;
            var endOfZiploc = contentString.find("\"", indexOfZiploc);
            // Add on the first part of the project page URL to get a complete URL
            var endOfProjectPageDomain = projectPage.find("/", 8);
            var projectPageDomain = projectPage[0::endOfProjectPageDomain];
            return projectPageDomain + contentString[indexOfZiploc::endOfZiploc];
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to find downloadable zip file for addon. Skipping...\n");
            return "";
        }
    }

    public static object convertOldCurseURL(object addonpage)
    {
        try
        {
            // Curse has renamed some addons, removing the numbers from the URL. Rather than guess at what the new
            // name and URL is, just try to load the old URL and see where Curse redirects us to. We can guess at
            // the new URL, but they should know their own renaming scheme better than we do.
            var page = requests.get(addonpage);
            page.raise_for_status();
            return page.url;
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to find the current page for old URL \"" + addonpage + "\". Skipping...\n");
            return "";
        }
    }

    public static object getCurseVersion(object addonpage)
    {
        if (addonpage.Contains("/datastore"))
        {
            // For some reason, the dev for the DataStore addons stopped doing releases back around the
            // start of WoD and now just does alpha releases on the project page. So installing the
            // latest 'release' version gets you a version from 2014 that doesn't work properly. So
            // we'll grab the latest alpha from the project page instead.
            return getCurseDatastoreVersion(addonpage);
        }
        try
        {
            var page = requests.get(addonpage + "/files");
            page.raise_for_status();
            var contentString = str(page.content);
            var indexOfVer = contentString.find("file__name full") + 17;
            var endTag = contentString.find("</span>", indexOfVer);
            return contentString[indexOfVer::endTag].strip();
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to find version number for: " + addonpage);
            return "";
        }
    }

    public static object getCurseDatastoreVersion(object addonpage)
    {
        try
        {
            // First, look for the URL of the project file page
            var page = requests.get(addonpage);
            page.raise_for_status();
            var contentString = str(page.content);
            var endOfProjectPageURL = contentString.find("\">Visit Project Page");
            var indexOfProjectPageURL = contentString.rfind("<a href=\"", 0, endOfProjectPageURL) + 9;
            var projectPage = contentString[indexOfProjectPageURL::endOfProjectPageURL];
            // Now just call getCurseProjectVersion with the URL we found
            return getCurseProjectVersion(projectPage);
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to find alpha version number for: " + addonpage);
        }
    }

    // Curse Project
    public static object curseProject(object addonpage)
    {
        try
        {
            // Apparently the Curse project pages are sometimes sending people to WowAce now.
            // Check if the URL forwards to WowAce and use that URL instead.
            var page = requests.get(addonpage);
            page.raise_for_status();
            if (page.url.startswith("https://www.wowace.com/projects/"))
            {
                return wowAceProject(page.url);
            }
            return addonpage + "/files/latest";
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to find downloadable zip file for addon. Skipping...\n");
            return "";
        }
    }

    public static object getCurseProjectVersion(object addonpage)
    {
        try
        {
            var page = requests.get(addonpage + "/files");
            if (page.status_code == 404)
            {
                // Maybe the project page got moved to WowAce?
                page = requests.get(addonpage);
                page.raise_for_status();
                page = requests.get(page.url + "/files");
                page.raise_for_status();
            }
            var contentString = str(page.content);
            var startOfTable = contentString.find("project-file-list-item");
            var indexOfVer = contentString.find("data-name=\"", startOfTable) + 11;
            var endTag = contentString.find("\">", indexOfVer);
            return contentString[indexOfVer::endTag].strip();
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to find version number for: " + addonpage);
            return "";
        }
    }

    // WowAce Project
    public static object wowAceProject(object addonpage)
    {
        try
        {
            return addonpage + "/files/latest";
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to find downloadable zip file for addon. Skipping...\n");
            return "";
        }
    }

    public static object getWowAceProjectVersion(object addonpage)
    {
        try
        {
            var page = requests.get(addonpage + "/files");
            page.raise_for_status();
            var contentString = str(page.content);
            var startOfTable = contentString.find("project-file-list-item");
            var indexOfVer = contentString.find("data-name=\"", startOfTable) + 11;
            var endTag = contentString.find("\">", indexOfVer);
            return contentString[indexOfVer::endTag].strip();
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to find version number for: " + addonpage);
            return "";
        }
    }

    // Tukui
    public static object tukui(object addonpage)
    {
        try
        {
            return addonpage + "/-/archive/master/elvui-master.zip";
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to find downloadable zip file for addon. Skipping...\n");
            return "";
        }
    }

    public static object getTukuiVersion(object addonpage)
    {
        try
        {
            var response = requests.get(addonpage);
            response.raise_for_status();
            var content = str(response.content);
            var match = re.search(@"<div class=""commit-sha-group"">\\n<div class=""label label-monospace"">\\n(?P<hash>[^<]+?)\\n</div>", content);
            var result = "";
            if (match)
            {
                result = match.group("hash");
            }
            return result.strip();
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to find version number for: " + addonpage);
            Console.WriteLine(err);
            return "";
        }
    }

    // Wowinterface
    public static object wowinterface(object addonpage)
    {
        var downloadpage = addonpage.replace("info", "download");
        try
        {
            var page = requests.get(downloadpage + "/download");
            page.raise_for_status();
            var contentString = str(page.content);
            var indexOfZiploc = contentString.find("Problems with the download? <a href=\"") + 37;
            var endQuote = contentString.find("\"", indexOfZiploc);
            return contentString[indexOfZiploc::endQuote];
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to find downloadable zip file for addon. Skipping...\n");
            return "";
        }
    }

    public static object getWowinterfaceVersion(object addonpage)
    {
        try
        {
            var page = requests.get(addonpage);
            page.raise_for_status();
            var contentString = str(page.content);
            var indexOfVer = contentString.find("id=\"version\"") + 22;
            var endTag = contentString.find("</div>", indexOfVer);
            return contentString[indexOfVer::endTag].strip();
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to find version number for: " + addonpage);
            return "";
        }
    }
}