﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Xamarin.Essentials;
using SQLite;
using SQLitePCL;
using flight_director.ViewModels;
using flight_director.Models;
using flight_director.Services;
using Xamarin.Forms;
using static System.Math;
using Command = Xamarin.Forms.Command;
using flight_director.Views;

[assembly: Dependency(typeof(FlightLineService))]

namespace flight_director.ViewModels
{
    public class NavigationViewModel : ViewModelBase
    {
        // UI Bindings
        private string version = AppInfo.VersionString;
        public string Version
        {
            get => version;
            set => SetProperty(ref version, value);
        }

        private bool enableflyto = true;
        public bool EnableFlyTo
        {
            get => enableflyto;
            set => SetProperty(ref enableflyto, value);
        }

        private bool abort = false;
        public bool Abort
        {
            get => abort;
            set => SetProperty(ref abort, value);
        }

        private double heading = 0;
        public double Heading
        {
            get => heading;
            set => SetProperty(ref heading, value);
        }

        private double headingcomp = 0;
        public double HeadingComp
        {
            get => headingcomp;
            set => SetProperty(ref headingcomp, value);
        }

        private int headingdisp = 0;
        public int HeadingDisp
        {
            get => headingdisp;
            set => SetProperty(ref headingdisp, value);
        }

        private int lineid = 0;
        public int LineID
        {
            get => lineid;
            set => SetProperty(ref lineid, value);
        }

        private string status = "Standby";
        public string Status
        {
            get => status;
            set => SetProperty(ref status, value);
        }

        private string statuscolor = "White";
        public string StatusColor
        {
            get => statuscolor;
            set => SetProperty(ref statuscolor, value);
        }

        private string buttoncolor = "Gray";
        public string ButtonColor
        {
            get => buttoncolor;
            set => SetProperty(ref buttoncolor, value);
        }

        private int curline = 0;
        public int CurLine
        {
            get => curline;
            set => SetProperty(ref curline, value);
        }

        private int distrem = 0;
        public int DistRem
        {
            get => distrem;
            set => SetProperty(ref distrem, value);
        }

        private string distremdisp = "Dist Rem: ";
        public string DistRemDisp
        {
            get => distremdisp;
            set => SetProperty(ref distremdisp, value);
        }

        private double course = 0;
        public double Course
        {
            get => course;
            set => SetProperty(ref course, value);
        }

        private double currentlat = 0;
        public double CurrentLat
        {
            get => currentlat;
            set => SetProperty(ref currentlat, value);
        }

        private double currentlon = 0;
        public double CurrentLon
        {
            get => currentlon;
            set => SetProperty(ref currentlon, value);
        }

        private double prevlat = 0;
        public double PrevLat
        {
            get => prevlat;
            set => SetProperty(ref prevlat, value);
        }

        private double prevlon = 0;
        public double PrevLon
        {
            get => prevlon;
            set => SetProperty(ref prevlon, value);
        }

        private double targetlat = 0;
        public double TargetLat
        {
            get => targetlat;
            set => SetProperty(ref targetlat, value);
        }

        private double targetlon = 0;
        public double TargetLon
        {
            get => targetlon;
            set => SetProperty(ref targetlon, value);
        }

        private double deviation = 0;
        public double Deviation
        {
            get => deviation;
            set => SetProperty(ref deviation, value);
        }

        private double deviationnum = 0;
        public double DeviationNum
        {
            get => deviationnum;
            set => SetProperty(ref deviationnum, value);
        }

        private string deviationdisp = "Deviation:";
        public string DeviationDisp
        {
            get => deviationdisp;
            set => SetProperty(ref deviationdisp, value);
        }

        private double transx = 0;
        public double TransX
        {
            get => transx;
            set => SetProperty(ref transx, value);
        }

        private double transy = 0;
        public double TransY
        {
            get => transy;
            set => SetProperty(ref transy, value);
        }

        private string trackcourse = "Line's course:";
        public string TrackCourse
        {
            get => trackcourse;
            set => SetProperty(ref trackcourse, value);
        }

        private string accdisp = "Acc:";
        public string AccDisp
        {
            get => accdisp;
            set => SetProperty(ref accdisp, value);
        }

        private int acc=0;
        public int Acc
        {
            get => acc;
            set => SetProperty(ref acc, value);
        }

        // General Settings
        public  int WPRadius
        {
            get => Preferences.Get(nameof(WPRadius),500);
            set
            {
                Preferences.Set(nameof(WPRadius), value);
                OnPropertyChanged(nameof(WPRadius));
            }
        }

        public int FeetperBar
        {
            get => Preferences.Get(nameof(FeetperBar), 15);
            set
            {
                Preferences.Set(nameof(FeetperBar), value);
                OnPropertyChanged(nameof(FeetperBar));
            }
        }

        public int HeadingThreshold
        {
            get => Preferences.Get(nameof(HeadingThreshold), 10);
            set
            {
                Preferences.Set(nameof(HeadingThreshold), value);
                OnPropertyChanged(nameof(HeadingThreshold));
            }
        }

    // Some functions
    public double Rad2Deg(double rad)
        {
            return rad * 180 / Math.PI;
        }

        public double Deg2Rad(double deg)
        {
            return deg * Math.PI / 180;
        }

        public double CalcCourse_rad(double startlat, double startlon, double endlat, double endlon)
        {
            double dL = Deg2Rad(endlon - startlon);
            double x = Sin(dL) * Cos(Deg2Rad(endlat));
            double y = Cos(Deg2Rad(startlat)) * Sin(Deg2Rad(endlat)) - Sin(Deg2Rad(startlat)) * Cos(Deg2Rad(endlat)) * Cos(dL);
            double bearing_rad = Atan2(x, y);
            double bearing_deg = Rad2Deg(bearing_rad);
            return Deg2Rad((bearing_deg + 360) % 360);

        }

        public NavigationViewModel()
        {
            Flyto = new AsyncCommand(Navigate);
            AbortNav = new Command(CancleNav);
            ToSettings = new AsyncCommand(GotoSettings);
        }

        public AsyncCommand ToSettings { get; }
        public AsyncCommand Flyto { get; }
        public ICommand AbortNav { get; }

        void CancleNav()
        {
            Abort = true;
        }

        async Task GotoSettings()
        {
            await Shell.Current.GoToAsync(nameof(Settings));
        }

        async Task Navigate()
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(1));
            /* Enter FlyTo Mode
             * flight_director navigate between current position and the start coordinate of the line*/
            //Initialize current position
            StatusColor = "White";
            EnableFlyTo = false;
            Status = "Standby";
            var cur_pos = await Geolocation.GetLocationAsync(request);
            if (cur_pos == null)
            {
                Status = "No GPS";
                StatusColor = "Red";
                EnableFlyTo = true;
                return;
            }
            CurrentLat = cur_pos.Latitude;
            CurrentLon = cur_pos.Longitude;

            //Parse selected line info
            if (CurLine <=0)
            {
                Status = "Invalid Line";
                StatusColor = "Red";
                EnableFlyTo = true;
                return;
            } //Check if new line has been entered (<=0)
            var cur_line = await FlightLineService.GetLine(CurLine);
            if (cur_line == null) //Check if the entered line is valid (id < total number of line in database)
            {
                Status = "Invalid Line";
                StatusColor = "Red";
                EnableFlyTo = true;
                return;
            }
            LineID = cur_line.ID;
            TargetLat = cur_line.StartLat;
            TargetLon = cur_line.StartLon;
            double track_course = CalcCourse_rad(CurrentLat, CurrentLon, TargetLat, TargetLon);
            double bearing_track = CalcCourse_rad(TargetLat, TargetLon, CurrentLat, CurrentLon);
            double trackcourse = CalcCourse_rad(cur_line.StartLat, cur_line.StartLon, cur_line.EndLat, cur_line.EndLon);
            TrackCourse = $"Line's course: {(int)Rad2Deg(trackcourse)}";


            //Begin navigating in Flyto
            Status = $"Flying to line {LineID}";
            double rem_miles = Location.CalculateDistance(CurrentLat,CurrentLon,TargetLat,TargetLon,DistanceUnits.Miles);
            double rem_ft = rem_miles * 5280;
            DistRem = (int)rem_ft;
            ButtonColor = "Red";

            // FlyTo Loop
            while (DistRem > WPRadius) //Check if beginning of first waypoint is reached
            {
                if (Abort==true)
                {
                    Abort = false;
                    EnableFlyTo = true;
                    Status = "Standby";
                    ButtonColor = "Gray";
                    return;
                }
                cur_pos = await Geolocation.GetLocationAsync(request);
                if (cur_pos != null && cur_pos.Accuracy < 30)
                {
                    CurrentLat = cur_pos.Latitude;
                    CurrentLon = cur_pos.Longitude;
                    if ((int)(5280 * Location.CalculateDistance(CurrentLat, CurrentLon, PrevLat, PrevLon, DistanceUnits.Miles)) > HeadingThreshold)
                    {
                        Heading = (double)Rad2Deg(CalcCourse_rad(PrevLat, PrevLon, CurrentLat, CurrentLon));
                        PrevLat = CurrentLat;
                        PrevLon = CurrentLon;
                        Course = (double)-Rad2Deg(Deg2Rad(Heading) - track_course);
                    }
                    DistRem = (int)( 5280 * Location.CalculateDistance(CurrentLat, CurrentLon, TargetLat, TargetLon, DistanceUnits.Miles));
                    double bearing_cur_pos = CalcCourse_rad(TargetLat,TargetLon,CurrentLat,CurrentLon);
                    DeviationNum = DistRem * Sin(bearing_cur_pos - bearing_track);
                    Deviation = DeviationNum * (35 / FeetperBar);
                    if (Abs(Deviation) > 110)
                    {
                        Deviation = Sign(Deviation) * 110;
                    }
                    TransX = Deviation * Cos(Deg2Rad(Course));
                    TransY = Deviation * Sin(Deg2Rad(Course));
                    Acc = (int)(cur_pos.Accuracy * 3.28084);
                    HeadingComp = -Heading;
                    HeadingDisp = (int)Heading;
                    DeviationDisp = $"Deviation: {Abs((int)DeviationNum)} ft";
                    DistRemDisp = $"Dist Rem: {DistRem} ft";
                    AccDisp = $"Acc: {Acc} ft";
                }
                await Task.Delay(15);
                
            }

            // When arrive at starting waypoint of line
            // Set the start and end coordinate to the lines coordinates
            CurrentLat = cur_line.StartLat;
            CurrentLon = cur_line.StartLon;
            TargetLat = cur_line.EndLat;
            TargetLon = cur_line.EndLon;
            bearing_track = CalcCourse_rad(TargetLat, TargetLon, CurrentLat, CurrentLon);
            //Recalculate remaining distance
            rem_miles = Location.CalculateDistance(CurrentLat, CurrentLon, TargetLat, TargetLon, DistanceUnits.Miles);
            rem_ft = rem_miles * 5280;
            DistRem = (int)rem_ft;
            // Update status to Navigate mode (follow the refeence trajectory)
            ButtonColor = "Green";
            Status = $"Flying line {LineID}";
            //Line Navigating loop
            while (DistRem > WPRadius) //Check if beginning of first waypoint is reached
            {
                if (Abort == true)
                {
                    Abort = false;
                    EnableFlyTo = true;
                    Status = "Standby";
                    ButtonColor = "Gray";
                    return;
                }
                cur_pos = await Geolocation.GetLocationAsync(request);
                if (cur_pos != null && cur_pos.Accuracy < 30)
                {
                    CurrentLat = cur_pos.Latitude;
                    CurrentLon = cur_pos.Longitude;
                    if ((int)(5280 * Location.CalculateDistance(CurrentLat, CurrentLon, PrevLat, PrevLon, DistanceUnits.Miles)) > HeadingThreshold)
                    {
                        Heading = (double)Rad2Deg(CalcCourse_rad(PrevLat, PrevLon, CurrentLat, CurrentLon));
                        PrevLat = CurrentLat;
                        PrevLon = CurrentLon;
                        Course = (double)-Rad2Deg(Deg2Rad(Heading) - trackcourse);
                    }
                    DistRem = (int)(5280 * Location.CalculateDistance(CurrentLat, CurrentLon, TargetLat, TargetLon, DistanceUnits.Miles));
                    double bearing_cur_pos = CalcCourse_rad(TargetLat, TargetLon, CurrentLat, CurrentLon);
                    DeviationNum = DistRem * Sin(bearing_cur_pos - bearing_track);
                    Deviation = DeviationNum * (35 / FeetperBar);
                    if (Abs(Deviation) > 110)
                    {
                        Deviation = Sign(Deviation) * 110;
                    }
                    TransX = Deviation * Cos(Deg2Rad(Course));
                    TransY = Deviation * Sin(Deg2Rad(Course));
                    Acc = (int)(cur_pos.Accuracy * 3.28084);
                    HeadingComp = -Heading;
                    HeadingDisp = (int)Heading;
                    DeviationDisp = $"Deviation: {Abs((int)(DeviationNum ))} ft";
                    DistRemDisp = $"Dist Rem: {DistRem} ft";
                    AccDisp = $"Acc: {Acc} ft";
                }
                await Task.Delay(15);

            }
            //Finish navigating
            EnableFlyTo = true;
            ButtonColor = "Gray";
            Status = "Stand By";
        }



    }
}
