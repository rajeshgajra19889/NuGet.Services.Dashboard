﻿using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using NuGetGallery.Operations.Common;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Xml;


namespace NuGetGallery.Operations
{
    [Command("CreateTrafficManagerStatusOverviewReportTask", "Creates report for highlevel status of Traffic manager", AltName = "ctmort")]
    public class CreateTrafficManagerStatusOverviewReportTask : StorageTask
    {
        [Option("SubsciptionId", AltName = "id")]
        public string SubscriptionId { get; set; }

        [Option("ProfileName", AltName = "name")]
        public string ProfileName { get; set; }

        [Option("CertificateName", AltName = "cername")]
        public string CertificateName { get; set; }
        public override void ExecuteCommand()
        {
            X509Certificate cert = X509Certificate.CreateFromCertFile(CertificateName);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("https://management.core.windows.net/{0}/services/WATM/profiles/{1}/definitions", SubscriptionId, ProfileName));
            request.ClientCertificates.Add(cert);
            request.Headers.Add("x-ms-version: 2014-02-01");
            request.PreAuthenticate = true;
            request.Method = "GET";
            WebResponse respose = request.GetResponse();
            //Schema of the response would be as specified in http://msdn.microsoft.com/en-us/library/azure/hh758251.aspx
            Console.WriteLine(respose);
            using (var reader = new StreamReader(respose.GetResponseStream()))
            {               
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(reader.ReadToEnd());
                XmlNodeList parentNode = doc.GetElementsByTagName("Endpoint", "http://schemas.microsoft.com/windowsazure");
                List<Tuple<string, string>> endpointValues = new List<Tuple<string, string>>();
                foreach (XmlNode node in parentNode)
                {
                    string endpointName = node.ChildNodes[0].InnerText;
                    string endpointStatus = node.ChildNodes[3].InnerText;                             
                    Console.WriteLine(string.Format("End point name {0}, status {1}", endpointName, endpointStatus));
                    endpointValues.Add(new Tuple<string, string>(endpointName, endpointStatus));
                    if(!endpointStatus.Equals("Online",StringComparison.OrdinalIgnoreCase))
                    {
                        new SendAlertMailTask
                        {
                            AlertSubject = string.Format("Error: Traffic manager endpoint alert activated for {0}",endpointName),
                            Details = string.Format("The status of the endpoint {0} monitoring by traffic manager {1} is {2}", endpointName, ProfileName, endpointStatus),
                            AlertName = "Error: Alert for TrafficManagerEndpoint",
                            Component = "TrafficManager",
                            Level = "Error"
                        }.ExecuteCommand();
                    }
                   
                }
                     JArray reportObject = ReportHelpers.GetJson(endpointValues);
                     ReportHelpers.CreateBlob(StorageAccount, "TrafficManagerStatus.json", ContainerName, "application/json", ReportHelpers.ToStream(reportObject));
            }
        }



    }
}

