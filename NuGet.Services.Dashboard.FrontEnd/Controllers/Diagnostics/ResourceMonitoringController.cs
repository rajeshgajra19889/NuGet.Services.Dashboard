﻿using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using NuGetDashboard.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NuGetDashboard.Controllers.Diagnostics
{
    /// <summary>
    /// Provides details about the resource utilization on the server : CPU, memory and DB.
    /// </summary>
    public class ResourceMonitoringController : Controller
    {    
        [HttpGet]
        public ActionResult DBCPUTime()
        {
            return PartialView("~/Views/Shared/PartialChart.cshtml", ChartingUtilities.GetLineChartFromBlobName("DBCPUTime" + string.Format("{0:MMdd}", DateTimeUtility.GetPacificTime()), "DBCPUTimeInSeconds"));
        }

        [HttpGet]
        public ActionResult DBRequests()
        {                   
            return PartialView("~/Views/Shared/PartialChart.cshtml", ChartingUtilities.GetLineChartFromBlobName("DBRequests" + string.Format("{0:MMdd}", DateTimeUtility.GetPacificTime()), "DBRequests",12));
        }

        [HttpGet]
        public ActionResult DBConnections()
        {
            return PartialView("~/Views/Shared/PartialChart.cshtml", ChartingUtilities.GetLineChartFromBlobName("DBConnections" + string.Format("{0:MMdd}", DateTimeUtility.GetPacificTime()), "DBConnections", 12));           
        }      

        [HttpGet]
        public ActionResult DBCPUTimeThisWeek()
        {
            string[] blobNames = new string[4];
            for (int i = 0; i < 4; i++)
                blobNames[i] = "DBCPUTime" + string.Format("{0:MMdd}", DateTimeUtility.GetPacificTime().AddDays(-i));
            return PartialView("~/Views/Shared/PartialChart.cshtml", ChartingUtilities.GetLineChartFromBlobName(blobNames, "DBCPUTimeInSeconds", 50, 400));
        }
        [HttpGet]
        public ActionResult DBRequestsThisWeek()
        {
            string[] blobNames = new string[4];
            for (int i = 0; i < 4; i++)
                blobNames[i] = "DBRequests" + string.Format("{0:MMdd}", DateTimeUtility.GetPacificTime().AddDays(-i));
            return PartialView("~/Views/Shared/PartialChart.cshtml", ChartingUtilities.GetLineChartFromBlobName(blobNames, "DBRequests", 50, 600));
        }
        [HttpGet]
        public ActionResult DBConnectionsThisWeek()
        {
            string[] blobNames = new string[4];
            for (int i = 0; i < 4; i++)
                blobNames[i] = "DBConnections" + string.Format("{0:MMdd}", DateTimeUtility.GetPacificTime().AddDays(-i));
            return PartialView("~/Views/Shared/PartialChart.cshtml", ChartingUtilities.GetLineChartFromBlobName(blobNames, "DBConnections", 50, 600));
        }       

        private ActionResult GetChart(string blobName)
        {   
            return PartialView("~/Views/Shared/PartialChart.cshtml", ChartingUtilities.GetLineChartFromBlobName(blobName,blobName));
        }

    }
}
