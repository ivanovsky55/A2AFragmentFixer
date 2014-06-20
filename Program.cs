﻿using System;
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
        static void Main(string[] args)
        {
            //string fixPath = args[0];
            string fixPath = @"C:\Users\v-ivayal\Desktop\Temp\XMLs\Test\OCMS2DX-Spanish";
            string usPath = @"C:\Users\v-ivayal\Desktop\Temp\XMLs\Test\OCMS2DX-English";
            FixFolder(fixPath, usPath);
        }

        private static void FixFolder(string marketFolderPath, string usFolderPath)
        {
            var doc = XDocument.Load(Path.Combine(marketFolderPath, "ConvertAssets.xml"));

            //Iterate each of the Lead assets that have FR Dependent Assets
            //Only do this for HA, VA, RZ
            string query = "//DefinedAssets/Asset[(contains(AssetId,'HA') or contains(AssetId,'VA') or contains(AssetId,'RZ')) and Converted='True' and contains(ResolvedDependencies, 'FR')]";
            foreach (var xe in doc.XPathSelectElements(query))
            {
                var assetId = TryGetXValue(xe, "AssetId");
                if (assetId.Contains("HA") || assetId.Contains("VA") || assetId.Contains("RZ"))
                {
                    var convertedPath = Path.Combine(marketFolderPath, "Converted");
                    var filename = GetNewestDduemlPath(convertedPath, assetId);
                    if (String.IsNullOrEmpty(filename))
                    {
                        //Log it?
                        continue;
                    }

                    var relatedAssets = TryGetXValue(xe, "ResolvedDependencies").Split(',');
                    List<string> fragmentIds = new List<string>();
                    foreach (var id in relatedAssets)
                        if (id.Contains("FR")) fragmentIds.Add(id);

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
                var intlFragmentPath = GetNewestDduemlPath(intlConvPath, frId);
                var usFragmentPath = GetNewestDduemlPath(UsConvPath, frId);
                if (String.IsNullOrWhiteSpace(intlFragmentPath) || String.IsNullOrWhiteSpace(usFragmentPath))
                {
                    //Log?
                    continue;
                }
                //Search if the structure exists in the Intl/Converted/HA.xml file

                if (IsXmlSubset(fileName, usFragmentPath))
                { }

                //XElement intlFragment = ExtractFragment(intlFragmentPath);


                //////If it exists...
                ////////it's an unlocalized fragment, so we grab Intl/Converted/FR.xml and replace the identified English FR
                //////If it doesn't exist, that fragment is localized or non-existant, so ignore it and check the next fragment for that article
            }
        }

        private static bool IsXmlSubset(string articlePath, string fragmentPath)
        {
            bool res = false;

            //XElement usFragment = ExtractFragment(fragmentPath);
            //XDocument intlDoc = XDocument.Load(articlePath);

            //var namespaceManager = new XmlNamespaceManager(new NameTable());
            //namespaceManager.AddNamespace("def", "http://ddue.schemas.microsoft.com/authoring/2003/5");
            //namespaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink");

            //foreach (var xe in xDoc.XPathSelectElements("//default:link", namespaceManager))
            //{
            //}

            using (var r = File.OpenText(articlePath))
            {
                XPathDocument xd = new XPathDocument(XmlReader.Create(r));
                XPathNavigator xn = xd.CreateNavigator();

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xn.NameTable);
                nsmgr.AddNamespace("def", "http://ddue.schemas.microsoft.com/authoring/2003/5");

                XPathNodeIterator xni = xn.Select("//def:introduction", nsmgr);

                foreach (XPathNavigator nav in xni)
                {
                    Console.WriteLine(nav.Name);
                }
            }
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


        private static string GetNewestDduemlPath(string convFolder, string assetId)
        {
            var res = "";
            var files = Directory.GetFiles(convFolder, assetId + "*.xml");
            if (files.Length == 1)
                res = files[0];
            else
            {
                if (files.Length != 0)
                {
                    res = files[0];
                    foreach (var file in files)
                    {
                        string regex = ".*\\.(\\d*)\\.(\\d*)\\..*";
                        Regex reg = new Regex(regex);

                        var matchOld = reg.Match(Path.GetFileName(res));
                        var majorVersionOld = Convert.ToInt32(matchOld.Groups[1].Value);
                        var minorVersionOld = Convert.ToInt32(matchOld.Groups[2].Value);

                        var matchNew = reg.Match(Path.GetFileName(file));
                        var majorVersionNew = Convert.ToInt32(matchNew.Groups[1].Value);
                        var minorVersionNew = Convert.ToInt32(matchNew.Groups[2].Value);

                        //If new file has a higher major version, we take it
                        if (majorVersionOld < majorVersionNew)
                        {
                            res = file;
                        }
                        else
                        {
                            //If the major versions are the same, we may still take it...
                            if (majorVersionOld == majorVersionNew)
                            {
                                //if the new file has a minor version higher than the current, we take it
                                if (minorVersionOld < minorVersionNew)
                                {
                                    res = file;
                                }
                            }
                        }
                    }
                }
            }
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