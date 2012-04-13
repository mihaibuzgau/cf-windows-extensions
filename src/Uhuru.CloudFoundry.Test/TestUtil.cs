﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Xml;
using System.Threading;
using Uhuru.CloudFoundry.Adaptor.Objects;
using Uhuru.CloudFoundry.Adaptor.Objects.Packaging;
using Uhuru.CloudFoundry.Adaptor;
using System.Configuration;
using System.Security;

namespace Uhuru.CloudFoundry.Test
{
    class TestUtil
    {

        public static void DeleteUser(string username, List<string> directoriesCreated)
        {
            string target = ConfigurationManager.AppSettings["target"];
            CloudCredentialsEncryption encryptor = new CloudCredentialsEncryption();
            SecureString encryptedPassword = encryptor.Decrypt(ConfigurationManager.AppSettings["adminPassword"].ToString());
            CloudManager cloudManager = CloudManager.Instance();
            CloudTarget cloudTarget = new CloudTarget(ConfigurationManager.AppSettings["adminUsername"].ToString(), encryptedPassword, new Uri(target));
            CloudConnection cloudConnection = cloudManager.GetConnection(cloudTarget);

            User tempUser = cloudConnection.Users.First(usr => usr.Email == username);
            tempUser.Delete();
            foreach (string str in directoriesCreated)
            {
                Directory.Delete(str, true);
            }
        }

        public static CloudConnection CreateAndImplersonateUser(string username,string password)
        {
            string target = ConfigurationManager.AppSettings["target"];
            CloudCredentialsEncryption encryptor = new CloudCredentialsEncryption();
            SecureString encryptedPassword = encryptor.Decrypt(ConfigurationManager.AppSettings["adminPassword"].ToString());
            CloudManager cloudManager = CloudManager.Instance();
            CloudTarget cloudTarget = new CloudTarget(ConfigurationManager.AppSettings["adminUsername"].ToString(), encryptedPassword, new Uri(target));
            CloudConnection cloudConnection = cloudManager.GetConnection(cloudTarget);

            cloudConnection.CreateUser(username, password);

            cloudTarget = new CloudTarget(username, encryptor.Decrypt(password), new Uri(target));

            cloudConnection = cloudManager.GetConnection(cloudTarget);

            return cloudConnection;
        }


        public static void PushApp(string appName, string sourceDir, string url, List<string> directoriesCreated, CloudConnection cloudConnection, string vendor)
        {
            PushApp(appName, sourceDir, url, directoriesCreated, cloudConnection, vendor, null, null);
        }

        public static void PushApp(string appName, string sourceDir, string url, List<string> directoriesCreated, CloudConnection cloudConnection, string vendor, string serviceName, string path)
        {
            if (path == null)
                path = TestUtil.CopyFolderToTemp(sourceDir);
            directoriesCreated.Add(path);

            if (serviceName == null)
                serviceName = appName + "svc";

            CloudApplication cloudApp = new CloudApplication()
            {
                Name = appName,
                Urls = new string[1] { url },
                DeploymentPath = path,
                Deployable = true,
                Framework = "dotNet",
                InstanceCount = 1,
                Memory = 128,
                Runtime = "iis",
            };
            PushTracker pushTracker = new PushTracker();
            pushTracker.TrackId = Guid.NewGuid();
            cloudConnection.PushJob.Start(pushTracker, cloudApp);
            if (vendor != null)
            {
                cloudConnection.CreateProvisionedService(cloudConnection.SystemServices.FirstOrDefault(ss => ss.Vendor == vendor), serviceName, true);
                Thread.Sleep(1000);
            }

            App currentApp = cloudConnection.Apps.FirstOrDefault(app => app.Name == cloudApp.Name);

            ProvisionedService provisionedService = cloudConnection.ProvisionedServices.FirstOrDefault(ps => ps.Name == serviceName);
            if (vendor != null)
            {
                currentApp.BindService(provisionedService);
            }
            currentApp.Start();


        }

        public static string CopyFolderToTemp(string folder)
        {
            string tempFolder = Path.GetTempPath();
            string targetPath = Path.Combine(tempFolder, Guid.NewGuid().ToString());

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            if (Directory.Exists(folder))
            {
                DirectoryInfo source = new DirectoryInfo(folder);
                DirectoryInfo target = new DirectoryInfo(targetPath);

                CopyAll(source, target);
            }

            return targetPath;
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static string GetLocalIp()
        {
            IPHostEntry ipHostEntry;
            string localIP = string.Empty;
            ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in ipHostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }

            return localIP;
        }

        public static bool TestUrl(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            // Try 5 times to get the URL
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(2000);
                request.AllowAutoRedirect = false;
                try
                {
                    request.GetResponse();
                }
                catch (WebException)
                {
                    continue;
                }
                return true;
            }
            return false;
        }

        public static void UpdateWebConfigKey(string fileName, string key, string newValue)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            XmlNode appSettingsNode = xmlDoc.SelectSingleNode("configuration/appSettings");

            // Attempt to locate the requested setting.
            foreach (XmlNode childNode in appSettingsNode)
            {
                if (childNode.Attributes["key"].Value == key)
                {
                    childNode.Attributes["value"].Value = newValue;
                    break;
                }
            }
            xmlDoc.Save(fileName);
        }

        public static string GenerateAppName()
        {
            return Guid.NewGuid().ToString().Substring(0, 6);
        }
    }
}
