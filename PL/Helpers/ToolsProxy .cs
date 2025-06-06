using System;
using System.Reflection;

namespace PL.Helpers
{
    public static class ToolsProxy
    {
        public static double CalculateDistance(object latitude1, object longitude1, double latitude2, double longitude2)
        {
            // קבלת סוג המחלקה Tools
            var toolsType = typeof(BlApi.Factory).Assembly.GetType("BL.Helpers.Tools");
            if (toolsType == null)
                throw new InvalidOperationException("Tools class not found in BL.");

            // קבלת המתודה CalculateDistance
            var method = toolsType.GetMethod("CalculateDistance", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
                throw new InvalidOperationException("CalculateDistance method not found in Tools class.");

            // קריאה למתודה באמצעות Reflection
            return (double)method.Invoke(null, new object[] { latitude1, longitude1, latitude2, longitude2 });
        }
    }
}