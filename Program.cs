using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace A2AFragmentFixer
{
    class Program
    {
        static StreamWriter file;
        static void Main(string[] args)
        {
            //string fixPath = args[0];
            //string fixPath = @"C:\Users\v-ivayal\Desktop\Temp\XMLs\Test\OCMS2DX-Spanish";
            //string usPath = @"C:\Users\v-ivayal\Desktop\Temp\XMLs\Test\OCMS2DX-English";

            //string fixPath = @"\\WIN-E5U3RQA9QQH\MigrationTransfer\T1 Migration Kick-off\OCMS2Dx-Chinese-Simplified";

            //string usPath = @"\\hyunor01srv\share\SDSeeding\A2AMigrationOutput\OCMS2DX-English";
            //string logPath = @"C:\Users\v-ivayal\Desktop\Pseudo Trash\log4.txt";

            string usPath = @"\\hyunor01srv\share\SDSeeding\A2AMigrationOutput\OCMS2DX-English";
            string fixPath = "";
            string logPath = "";
            string mk = "";
            
            mk = "Chinese-Simplified";
            fixPath = @"\\WIN-E5U3RQA9QQH\MigrationTransfer\T1 Migration Kick-off\OCMS2Dx-" + mk;
            logPath = @"C:\Users\v-ivayal\Desktop\Pseudo Trash\log_" + mk + ".txt";
            file = new System.IO.StreamWriter(logPath);
            FixFolder(fixPath, usPath);
            file.Close();

            mk = "French";
            fixPath = @"\\WIN-E5U3RQA9QQH\MigrationTransfer\T1 Migration Kick-off\OCMS2Dx-" + mk;
            logPath = @"C:\Users\v-ivayal\Desktop\Pseudo Trash\log_" + mk + ".txt";
            file = new System.IO.StreamWriter(logPath);
            FixFolder(fixPath, usPath);
            file.Close();

            mk = "German";
            fixPath = @"\\WIN-E5U3RQA9QQH\MigrationTransfer\T1 Migration Kick-off\OCMS2Dx-" + mk;
            logPath = @"C:\Users\v-ivayal\Desktop\Pseudo Trash\log_" + mk + ".txt";
            file = new System.IO.StreamWriter(logPath);
            FixFolder(fixPath, usPath);
            file.Close();

            mk = "Japanese";
            fixPath = @"\\WIN-E5U3RQA9QQH\MigrationTransfer\T1 Migration Kick-off\OCMS2Dx-" + mk;
            logPath = @"C:\Users\v-ivayal\Desktop\Pseudo Trash\log_" + mk + ".txt";
            file = new System.IO.StreamWriter(logPath);
            FixFolder(fixPath, usPath);
            file.Close();

            mk = "Portuguese-Brazil";
            fixPath = @"\\WIN-E5U3RQA9QQH\MigrationTransfer\T1 Migration Kick-off\OCMS2Dx-" + mk;
            logPath = @"C:\Users\v-ivayal\Desktop\Pseudo Trash\log_" + mk + ".txt";
            file = new System.IO.StreamWriter(logPath);
            FixFolder(fixPath, usPath);
            file.Close();

            mk = "Russian";
            fixPath = @"\\WIN-E5U3RQA9QQH\MigrationTransfer\T1 Migration Kick-off\OCMS2Dx-" + mk;
            logPath = @"C:\Users\v-ivayal\Desktop\Pseudo Trash\log_" + mk + ".txt";
            file = new System.IO.StreamWriter(logPath);
            FixFolder(fixPath, usPath);
            file.Close();

            mk = "Spanish";
            fixPath = @"\\WIN-E5U3RQA9QQH\MigrationTransfer\T1 Migration Kick-off\OCMS2Dx-" + mk;
            logPath = @"C:\Users\v-ivayal\Desktop\Pseudo Trash\log_" + mk + ".txt";
            file = new System.IO.StreamWriter(logPath);
            FixFolder(fixPath, usPath);
            file.Close();

            Console.WriteLine("DONE!");
            Console.ReadKey();
        }

        static Dictionary<string, string> convertedDictionaryIntl;
        static Dictionary<string, string> convertedDictionaryUs;

        private static void FixFolder(string marketFolderPath, string usFolderPath)
        {
            var doc = XDocument.Load(Path.Combine(marketFolderPath, "ConvertAssets.xml"));

            convertedDictionaryUs = GetConvertedPathsDictionary(usFolderPath);
            convertedDictionaryIntl = GetConvertedPathsDictionary(marketFolderPath);

            //Iterate each of the Lead assets that have FR Dependent Assets
            //Only do this for HA, VA, RZ
            string query = "//DefinedAssets/Asset[(contains(AssetId,'HA') or contains(AssetId,'VA') or contains(AssetId,'RZ')) and Converted='True' and contains(ResolvedDependencies, 'FR')]";
            foreach (var xe in doc.XPathSelectElements(query))
            {
                var assetId = TryGetXValue(xe, "AssetId");
                if (assetId.Contains("HA") || assetId.Contains("VA") || assetId.Contains("RZ"))
                {
                    var convertedPath = Path.Combine(marketFolderPath, "Converted");
                    var filename = TryGetPath(assetId, convertedDictionaryIntl);
                    if (String.IsNullOrEmpty(filename))
                    {
                        //Log it?
                        continue;
                    }

                    var relatedAssets = TryGetXValue(xe, "ResolvedDependencies").Split(',');
                    List<string> fragmentIds = new List<string>();
                    foreach (var id in relatedAssets)
                        if (id.Contains("FR")) fragmentIds.Add(id.Trim());

                    var usConvertedPath = Path.Combine(usFolderPath, "Converted");
                    FixFile(filename, fragmentIds, convertedPath, usConvertedPath);
                }
            }
        }

        //HA010341571
        //FR102751215

        //FR000038914 This FR has <introduction /> but is not referenced by articles

        private static void FixFile(string fileName, List<string> fragmentIds, string intlConvPath, string UsConvPath)
        {
            //For each fragment in that article, Go to the English Converted folder and grab its FR.xml
            foreach (var frId in fragmentIds)
            {
                var intlFragmentPath = TryGetPath(frId, convertedDictionaryIntl);
                var usFragmentPath = TryGetPath(frId, convertedDictionaryUs);
                if (String.IsNullOrWhiteSpace(intlFragmentPath) || String.IsNullOrWhiteSpace(usFragmentPath))
                {
                    //Log?
                    continue;
                }
                //Search if the structure exists in the Intl/Converted/HA.xml file

                if (IsXmlSubset(fileName, usFragmentPath, intlFragmentPath))
                { }

                //XElement intlFragment = ExtractFragment(intlFragmentPath);


                //////If it exists...
                ////////it's an unlocalized fragment, so we grab Intl/Converted/FR.xml and replace the identified English FR
                //////If it doesn't exist, that fragment is localized or non-existant, so ignore it and check the next fragment for that article
            }
        }

        private static string StripFragment(string fr)
        {
            string fragmentText = fr.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "")
                        .Replace("<developerConceptualDocument xmlns=\"http://ddue.schemas.microsoft.com/authoring/2003/5\" xmlns:xlink=\"http://www.w3.org/1999/xlink\">", "")
                        .Replace("  <introduction>", "")
                        .Replace("  </introduction>", "")
                        .Replace("  <relatedTopics />", "")
                        .Replace("</developerConceptualDocument>", "")
                        .Replace(" address=\"__goback\"", "")
                        .Trim();
            return fragmentText;
        }

        private static string FlattenString(string s)
        {
            string res = s.Replace("  ", "").Replace("\r\n", "");
            RegexOptions options = ((RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline) | RegexOptions.IgnoreCase);

            //Remove <linkUri>.*?</linkUri> as they are inconsistent
            string regex = "<linkUri>.*?</linkUri>";
            Regex reg = new Regex(regex, options);
            res = reg.Replace(res, "");

            // xlink:href="6889df10-8cc4-4d8a-8a43-1477b0832911#__top"
            //string regex2 = "\\ xlink:href=.*?\\#__top\\\"";
            //Regex reg2 = new Regex(regex2, options);
            //res = reg2.Replace(res, "");

            return res;
        }

        //Works for Spanish HA010341571+FR102751215
        //HA102749523 FR104209832
        private static bool IsXmlSubset(string articlePath, string fragmentPath, string intlFragmentPath)
        {
            bool res = true;

            string flatUsFragText = FlattenString(StripFragment(File.ReadAllText(fragmentPath)));
            var flatArticleText = FlattenString(File.ReadAllText(articlePath));

            if (flatArticleText.Contains(flatUsFragText))
            {
                Console.WriteLine("Needs FR Fix: [" + Path.GetFileName(fragmentPath) + "] [" + Path.GetFileName(articlePath) + "]");
                file.WriteLine("Needs FR Fix: [" + Path.GetFileName(fragmentPath) + "] [" + Path.GetFileName(articlePath) + "]");
                return false;
                ////REPLACE US FRAG WITH INTL FRAG
                //var articleText = FlattenString(File.ReadAllText(articlePath));
                //string intlFragmentText = StripFragment(File.ReadAllText(intlFragmentPath));
                //string newArticleText = articleText.Replace("xxx", intlFragmentText);
                //File.WriteAllText(@"C:\Users\v-ivayal\Desktop\Pseudo Trash\new.xml", newArticleText);
            }
            else
            {
                string flatIntlFragText = FlattenString(StripFragment(File.ReadAllText(intlFragmentPath)));
                if (!flatArticleText.Contains(flatIntlFragText))
                {
                    Console.WriteLine("Neither FR is in file: [" + Path.GetFileName(intlFragmentPath) + "] [" + Path.GetFileName(articlePath) + "]");
                    file.WriteLine("Neither FR is in file: [" + Path.GetFileName(intlFragmentPath) + "] [" + Path.GetFileName(articlePath) + "]");
                    return false;
                }
            }
            Console.WriteLine(" OK: [Intl: " + Path.GetFileName(intlFragmentPath) + "] [" + Path.GetFileName(articlePath) + "]");
            file.WriteLine(" OK: [Intl: " + Path.GetFileName(intlFragmentPath) + "] [" + Path.GetFileName(articlePath) + "]");

            //XElement usFragment = ExtractFragment(fragmentPath);
            //XDocument intlDoc = XDocument.Load(fragmentPath);

            //var namespaceManager = new XmlNamespaceManager(new NameTable());
            //namespaceManager.AddNamespace("def", "http://ddue.schemas.microsoft.com/authoring/2003/5");
            //namespaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink");

            //foreach (var xe in intlDoc.XPathSelectElements("//def:para", namespaceManager))
            //{
            //}

            //using (var r = File.OpenText(fragmentPath))
            //{
            //    XPathDocument xd = new XPathDocument(XmlReader.Create(r));
            //    XPathNavigator xn = xd.CreateNavigator();

            //    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xn.NameTable);
            //    nsmgr.AddNamespace("def", "http://ddue.schemas.microsoft.com/authoring/2003/5");

            //    XPathNodeIterator xni = xn.Select("//def:introduction", nsmgr);

            //    foreach (XPathNavigator nav in xni)
            //    {
            //        Console.WriteLine(nav.Name);
            //        var x = nav.HasChildren;
            //        nav.MoveToFirstChild();
            //        x = nav.HasChildren;
            //        nav.MoveToFirstChild();
            //        x = nav.HasChildren;
            //        nav.MoveToFirstChild();
            //        nav.MoveToNext();
            //    }
            //}

            return res;
        }

        private static XElement ExtractFragment(string fragmentPath)
        {
            var xDoc = XDocument.Load(fragmentPath);
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("d", "http://ddue.schemas.microsoft.com/authoring/2003/5");
            var res = xDoc.XPathSelectElement("//d:introduction", namespaceManager);
            return res;
        }

        private static string TryGetXValue(XElement xe, string query)
        {
            string res = "";
            try
            {
                res = xe.XPathSelectElement(query).Value;
            }
            catch (NullReferenceException) { }
            return res;
        }


        //private static string GetNewestDduemlPath(string convFolder, string assetId)
        //{
        //    var res = "";
        //    var files = Directory.GetFiles(convFolder, assetId + "*.xml");
        //    if (files.Length == 1)
        //        res = files[0];
        //    else
        //    {
        //        if (files.Length != 0)
        //        {
        //            res = files[0];
        //            foreach (var file in files)
        //            {
        //                string regex = ".*\\.(\\d*)\\.(\\d*)\\..*";
        //                Regex reg = new Regex(regex);

        //                var matchOld = reg.Match(Path.GetFileName(res));
        //                var majorVersionOld = Convert.ToInt32(matchOld.Groups[1].Value);
        //                var minorVersionOld = Convert.ToInt32(matchOld.Groups[2].Value);

        //                var matchNew = reg.Match(Path.GetFileName(file));
        //                var majorVersionNew = Convert.ToInt32(matchNew.Groups[1].Value);
        //                var minorVersionNew = Convert.ToInt32(matchNew.Groups[2].Value);

        //                //If new file has a higher major version, we take it
        //                if (majorVersionOld < majorVersionNew)
        //                {
        //                    res = file;
        //                }
        //                else
        //                {
        //                    //If the major versions are the same, we may still take it...
        //                    if (majorVersionOld == majorVersionNew)
        //                    {
        //                        //if the new file has a minor version higher than the current, we take it
        //                        if (minorVersionOld < minorVersionNew)
        //                        {
        //                            res = file;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return res;
        //}

        private static Dictionary<string, string> GetConvertedPathsDictionary(string basePath)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            var convPath = Path.Combine(basePath, "Converted");
            foreach (var f in Directory.EnumerateFiles(convPath, "*.xml"))
            {
                //HA010186549.10.4.xml //VA102811844.0.14.xml
                var fName = Path.GetFileName(f);
                string regex = "(.*)\\.\\d*\\.\\d*\\..*";
                Regex reg = new System.Text.RegularExpressions.Regex(regex);
                var match = reg.Match(fName);
                var assetId = match.Groups[1].Value;
                if (!res.ContainsKey(assetId))
                {
                    res.Add(assetId, f);
                }
                else
                {
                    res[assetId] = DetermineNewest(assetId, res[assetId], f);
                }
            }
            return res;
        }

        private static string DetermineNewest(string assetId, string oldFile, string newFile)
        {
            string regex = ".*\\.(\\d*)\\.(\\d*)\\..*";
            Regex reg = new System.Text.RegularExpressions.Regex(regex);

            var matchOld = reg.Match(Path.GetFileName(oldFile));
            var majorVersionOld = Convert.ToInt32(matchOld.Groups[1].Value);
            var minorVersionOld = Convert.ToInt32(matchOld.Groups[2].Value);

            var matchNew = reg.Match(Path.GetFileName(newFile));
            var majorVersionNew = Convert.ToInt32(matchNew.Groups[1].Value);
            var minorVersionNew = Convert.ToInt32(matchNew.Groups[2].Value);

            //If new file has a higher major version, we update the dictionary
            if (majorVersionOld < majorVersionNew)
            {
                return newFile;
            }
            else
            {
                //If the major versions are the same, we may still take it...
                if (majorVersionOld == majorVersionNew)
                {
                    //if the new file has a minor version higher than the current, we update
                    if (minorVersionOld < minorVersionNew)
                    {
                        return newFile;
                    }
                }
            }
            return oldFile;
        }

        private static string TryGetPath(string assetId, Dictionary<string, string> dic)
        {
            string res = "";

            try
            {
                res = dic[assetId];
            }
            catch (KeyNotFoundException) { }

            return res;
        }
    }
}


//GetNewestFilename(@"C:\Users\v-ivayal\Desktop\Temp\OCMS2Dx\Converted", "HA1");


//XDocument xDoc = XDocument.Load(@"C:\Users\v-ivayal\Desktop\Temp\XMLs\_EmptyTop_HA102749557.0.8.xml");

//var namespaceManager = new  XmlNamespaceManager(new NameTable());
//namespaceManager.AddNamespace("default", "http://ddue.schemas.microsoft.com/authoring/2003/5");

//XNamespace ns = "";
//foreach (var xe in xDoc.XPathSelectElements("//default:link", namespaceManager))
//{
//}