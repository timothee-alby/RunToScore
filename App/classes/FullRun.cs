using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls.Maps;

namespace RunToScore.classes
{
    public class FullRun
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public double distance { get; set; }
        public double vitesse { get; set; }
        public int points { get; set; }
        public LocationCollection journey { get; set; }

        public string dateFormated { get { return startDate.ToString("ddd dd MMM"); } }
        public TimeSpan duration
        { 
            get 
            {
                TimeSpan duration = endDate.Subtract(startDate);
                return duration;
            }
        }
        public String durationFormated
        {
            get
            {
                TimeSpan duration = endDate.Subtract(startDate);
                if (duration.Hours <= 0)
                    return duration.Minutes + "m " + duration.Seconds + "s";
                else
                    return duration.Hours + "h " + duration.Minutes + "m";
            }
        }
        public int kCal
        {
            get
            {
                return (int)(distance * 70 / 1000);
            }
        }
        public FullRun()
        {
        }
        public FullRun Clone()
        {
            FullRun fr = new FullRun();
            fr.startDate = this.startDate;
            fr.endDate = this.endDate;
            fr.distance = this.distance;
            fr.vitesse = this.vitesse;
            fr.points = this.points;
            fr.journey = this.journey;
            return fr;
        }
    }
}
