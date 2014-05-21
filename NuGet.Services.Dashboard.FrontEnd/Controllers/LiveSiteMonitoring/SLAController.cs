﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NuGetDashboard.Utilities;
using DotNet.Highcharts.Helpers;
using NuGet.Services.Dashboard.Common;
using System.Web.Script.Serialization;

namespace NuGetDashboard.Controllers.LiveSiteMonitoring
{
    /// <summary>
    /// Provides details about the server side SLA : Error rate ans requests per hour.
    public class SLAController : Controller
    {
        public ActionResult Index()
        {            
            return PartialView("~/Views/SLA/SLA_Index.cshtml" );
        }

        [HttpGet]
        public ActionResult Details()
        {
            return PartialView("~/Views/SLA/SLA_Details.cshtml");
        }
     
        [HttpGet]
        public ActionResult ErrorsThisWeek()
        {
            string[] blobNames = new string[8];
            for (int i = 0; i < 8; i++)
                blobNames[i] = "ErrorRate" + string.Format("{0:MMdd}", DateTimeUtility.GetPacificTimeNow().AddDays(-i));
            return PartialView("~/Views/Shared/PartialChart.cshtml", ChartingUtilities.GetLineChartFromBlobName(blobNames, "ErrorsPerHour", 24, 800));
        }

        [HttpGet]
        public ActionResult RequestsThisWeek()
        {
            string[] blobNames = new string[8];
            for (int i = 0; i < 8;i++)
                   blobNames[i] = "IISRequests" + string.Format("{0:MMdd}", DateTimeUtility.GetPacificTimeNow().AddDays(-i));
            return PartialView("~/Views/Shared/PartialChart.cshtml", ChartingUtilities.GetLineChartFromBlobName(blobNames, "RequestsPerHour", 24, 800));
        }

        [HttpGet]
        public ActionResult RequestsToday()
        {
            List<Tuple<string, string, double>> scenarios = GetRequestsData(String.Format("{0:MMdd}", DateTime.Now));
            return PartialView("~/Views/SLA/SLA_RequestDetails.cshtml", scenarios);
        }

        [HttpGet]
        public ActionResult AverageRequestPerHourTrendThisWeek()
        {
            List<string> value = new List<string>();
            Dictionary<string, List<object>> request = new Dictionary<string, List<object>>();
            List<DotNet.Highcharts.Options.Series> seriesSet = new List<DotNet.Highcharts.Options.Series>();
            DateTime start = DateTimeUtility.GetPacificTimeNow().AddDays(-7);
            for (int i = 0; i < 8; i++)
            {
                string date = string.Format("{0:MMdd}", start.AddDays(i));
                List<Tuple<string, string, double>> scenarios = GetRequestsData(date);
                value.Add(date);
                for (int j = 1; j < scenarios.Count; j++)
                {
                    if (request.ContainsKey(scenarios[j].Item1))
                    {
                        request[scenarios[j].Item1].Add(scenarios[j].Item2);
                    }
                    else
                    {
                        List<object> Yvalue = new List<object>();
                        Yvalue.Add(scenarios[j].Item2);
                        request.Add(scenarios[j].Item1, Yvalue);
                    }
                }
            }
           
            foreach (KeyValuePair<string, List<object>> each in request)
            {
                seriesSet.Add(new DotNet.Highcharts.Options.Series
                {
                    Data = new Data(each.Value.ToArray()),
                    Name = each.Key.Replace(" ","_")
                });
            }
            DotNet.Highcharts.Highcharts chart = ChartingUtilities.GetLineChart(seriesSet, value, "WeeklyAvgRequestPerHourTrend", 500);
            return PartialView("~/Views/Shared/PartialChart.cshtml", chart);
        }

        [HttpGet]
        public ActionResult AverageTimeTakenInMsTrendThisWeek()
        {
            List<string> value = new List<string>();
            Dictionary<string, List<object>> time = new Dictionary<string, List<object>>();
            List<DotNet.Highcharts.Options.Series> seriesSet = new List<DotNet.Highcharts.Options.Series>();
            DateTime start = DateTimeUtility.GetPacificTimeNow().AddDays(-7);
            for (int i = 0; i < 8; i++)
            {
                string date = string.Format("{0:MMdd}", start.AddDays(i));
                List<Tuple<string, string, double>> scenarios = GetRequestsData(date);
                value.Add(date);
                for (int j = 1; j < scenarios.Count; j++)
                {
                    if (time.ContainsKey(scenarios[j].Item1))
                    {
                        time[scenarios[j].Item1].Add(scenarios[j].Item3.ToString());
                    }

                    else
                    {
                        List<object> Yvalue = new List<object>();
                        Yvalue.Add(scenarios[j].Item3.ToString());
                        time.Add(scenarios[j].Item1, Yvalue);
                    }
                }
            }

            foreach (KeyValuePair<string, List<object>> each in time)
            {
                seriesSet.Add(new DotNet.Highcharts.Options.Series
                {
                    Data = new Data(each.Value.ToArray()),
                    Name = each.Key.Replace(" ", "_")
                });
            }
            DotNet.Highcharts.Highcharts chart = ChartingUtilities.GetLineChart(seriesSet, value, "WeeklyAvgTimeInMsTrend", 500);
            return PartialView("~/Views/Shared/PartialChart.cshtml", chart);
        }

        private List<Tuple<string, string, double>> GetRequestsData(string date)
        {
            Dictionary<string, string> dict = BlobStorageService.GetDictFromBlob("IISRequestDetails" + date + ".json");
            List<IISRequestDetails> requestDetails = new List<IISRequestDetails>();
            foreach (KeyValuePair<string, string> keyValuePair in dict)
            {
                requestDetails.AddRange(new JavaScriptSerializer().Deserialize<List<IISRequestDetails>>(keyValuePair.Value));
            }

            var requestGroups = requestDetails.GroupBy(item => item.ScenarioName);
            List<Tuple<string, string, double>> scenarios = new List<Tuple<string, string, double>>();
            foreach (IGrouping<string, IISRequestDetails> group in requestGroups)
            {
                scenarios.Add(new Tuple<string, string, double>(group.Key, Convert.ToInt32(group.Average(item => item.RequestsPerHour)).ToString(), Convert.ToInt32(group.Average(item => item.AvgTimeTakenInMilliSeconds))));
            }

            return scenarios;
        }

        [HttpGet]
        public ActionResult ErrorRate()
        {
            return PartialView("~/Views/Shared/PartialChart.cshtml", ChartingUtilities.GetLineChartFromBlobName("ErrorRate" + string.Format("{0:MMdd}", DateTimeUtility.GetPacificTimeNow()), "ErrorsPerHour"));
        }

        [HttpGet]
        public ActionResult Throughput()
        {
            return PartialView("~/Views/Shared/PartialChart.cshtml", ChartingUtilities.GetLineChartFromBlobName("IISRequests" + string.Format("{0:MMdd}", DateTimeUtility.GetPacificTimeNow()) , "RequestsPerHour"));
        }

        [HttpGet]
        public JsonResult GetCurrentThroughputStatus()
        {
            Dictionary<string, string> dict = BlobStorageService.GetDictFromBlob("IISRequests" + string.Format("{0:MMdd}",DateTimeUtility.GetPacificTimeNow()) + ".json");
            if (dict != null && dict.Count > 0)
                return Json(dict.Values.ElementAt(dict.Count - 1), JsonRequestBehavior.AllowGet);
            else
                return Json("N/A");
        }

        [HttpGet]
        public JsonResult GetCurrentErrorRateStatus()
        {
            Dictionary<string, string> dict = BlobStorageService.GetDictFromBlob("ErrorRate" + string.Format("{0:MMdd}", DateTimeUtility.GetPacificTimeNow()) + ".json");
            if (dict != null && dict.Count > 0)
                return Json(dict.Values.ElementAt(dict.Count - 1), JsonRequestBehavior.AllowGet);
            else
                return Json("N/A");
        }
    }
}
