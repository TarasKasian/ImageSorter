using System;
using System.Text;

namespace ImageSorter
{
    internal class Logger
    {
        public void TraceLog(StaticticsInfo staticticsInfo) 
        {
            string message = string.Empty;

            if (!staticticsInfo.IsOperationCompleted)
                message = BuildMessage(staticticsInfo);
            else
                message = BuildFinalStatisticsMessage(staticticsInfo);


            Console.WriteLine(message);
        }

        private static string BuildMessage(StaticticsInfo staticticsInfo) 
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" Files processed: " + staticticsInfo.FilesProcessed + " from " 
                      + staticticsInfo.FilesGeneral + " total,");
            sb.Append("\n Files left: " + staticticsInfo.FilesLeft);

            return sb.ToString();
        }

        private static string BuildFinalStatisticsMessage(StaticticsInfo staticticsInfo) 
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\n\n Operation completed.");
            sb.Append("\n Files processed: " + staticticsInfo.FilesProcessed + " from " 
                      + staticticsInfo.FilesGeneral + " total,");
            sb.Append("\n Operetion duration: " + staticticsInfo.OperationDuration.ToString(@"hh\:mm\:ss"));

            return sb.ToString();
        }
    }
}
