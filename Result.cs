using System;

namespace MonitorStudentsActivities
{
    public class Result
    {
        public int ID { get; set; }
        public int StudentID { get; set; }
        public int WorkID { get; set; }
        public int Attempt { get; set; }
        public double Score { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
    }
}
