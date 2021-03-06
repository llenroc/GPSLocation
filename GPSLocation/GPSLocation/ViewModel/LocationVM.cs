﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPSLocation.Model.Entities;
using GPSLocation.Utils;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms;

namespace GPSLocation.ViewModel
{
    public class LocationVM : ObservableBaseObject
    {
        private static string _tag = "LocationVM";

        private double lngX;
        public double LngX
        {
            get { return lngX; }
            set { lngX = value; OnPropertyChanged(); }
        }

        private double latY;
        public double LatY
        {
            get { return latY; }
            set { latY = value; OnPropertyChanged(); }
        }

        private double accuracy;
        public double Accuracy
        {
            get { return accuracy; }
            set { accuracy = value; OnPropertyChanged(); }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; OnPropertyChanged(); }
        }

        private bool _isEnableStartButton = true;
        public bool IsEnableStartButton
        {
            get { return _isEnableStartButton; }
            set { _isEnableStartButton = value; OnPropertyChanged(); }
        }

        private bool _isEnableStopButton = false;
        public bool IsEnableStopButton
        {
            get { return _isEnableStopButton; }
            set { _isEnableStopButton = value; OnPropertyChanged(); }
        }

        private bool _isStoppedGPSSearch = false;
        public bool IsStoppedGPSSearch
        {
            get { return _isStoppedGPSSearch; }
            set { _isStoppedGPSSearch = value; OnPropertyChanged(); }
        }

        private string _imageIndicator = "";
        public string ImageIndicator
        {
            get { return _imageIndicator; }
            set { _imageIndicator = value; OnPropertyChanged(); }
        }

        public Command StartSearchGPSCommand { get; set; }
        public Command StopSearchGPSCommand { get; set; }

        public LocationVM()
        {
            Log.GetInstance().Print(_tag, "LocationVM() >>> Se llama al constructor.");
            IsBusy = false;
            IsEnableStartButton = true;
            IsEnableStopButton = false;
            LngX = 0;
            LatY = 0;
            Accuracy = 0.00;
            ImageIndicator = "sred.png";
            IsStoppedGPSSearch = false;
            StartSearchGPSCommand = new Command((obj) => StartSearchGPS());
            StopSearchGPSCommand = new Command((obj) => StopSearchGPS());
        }

        private void ReinitGPSVariables()
        {
            IsBusy = true;
            IsEnableStartButton = false;
            IsEnableStopButton = false;
            LngX = 0;
            LatY = 0;
            Accuracy = 0.00;
            ImageIndicator = "sred.png";
            IsStoppedGPSSearch = false;
        }

        private async void StartSearchGPS()
        {
            if (!IsBusy)
            {
                ReinitGPSVariables();
                int precision = Settings.DesiredAccuracy;
                Log.GetInstance().Print(_tag, "StartSearchGPS() >>> Precisión deseada = {0}", precision);
                var locator = CrossGeolocator.Current;
                if (!locator.IsGeolocationAvailable)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Tu dispositivo no tiene soporte GPS", "Aceptar");
                    IsBusy = false;
                    IsEnableStartButton = true;
                    IsEnableStopButton = false;
                    IsStoppedGPSSearch = false;
                    return;
                }
                if (!locator.IsGeolocationEnabled)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "GPS no activado", "Aceptar");
                    IsBusy = false;
                    IsEnableStartButton = true;
                    IsEnableStopButton = false;
                    IsStoppedGPSSearch = false;
                    return;
                }
                locator.DesiredAccuracy = precision;
                int searchTimeInSeconds = Settings.SearchTimeInMinutes*60;
                DateTime startDateTime = DateTime.Now;
                DateTime finishDateTime = DateTime.Now.AddSeconds(searchTimeInSeconds);
                //int updateInterval = Settings.UpdateInterval;
                bool isFirstTime = true;
                while (startDateTime < finishDateTime && !IsStoppedGPSSearch)
                {
                    DateTime tempStart = DateTime.Now;
                    Position position = null;
                    try
                    {
                        position = await locator.GetPositionAsync(searchTimeInSeconds*1000);
                    }
                    catch (Exception ex)
                    {
                       Log.GetInstance().Print(_tag, "StartSearchGPS() >>> Error obteniendo ubicación. Detalle: {0}", ex.Message);
                    }
                    DateTime tempFinish = DateTime.Now;
                    TimeSpan tempDiff = tempFinish - tempStart;
                    startDateTime = startDateTime.AddSeconds(tempDiff.TotalSeconds);
                    if (isFirstTime)
                    {
                        ImageIndicator = "syellow.png";
                        LngX = position?.Longitude ?? 0.00;
                        LatY = position?.Latitude ?? 0.00;
                        Accuracy = Math.Round(position?.Accuracy ?? 0.00, 2);
                        isFirstTime = false;
                        if (Math.Abs(Accuracy) > 0 && Accuracy < precision)
                        {
                            ImageIndicator = "sgreen.png";
                            IsEnableStopButton = true;
                        }
                    }
                    else
                    {
                        if ((position != null && position.Accuracy < Accuracy) || (position != null && Math.Abs(Accuracy) < 1))
                        {
                            LngX = position.Longitude;
                            LatY = position.Latitude;
                            Accuracy = Math.Round(position.Accuracy, 2);
                            if (Math.Abs(Accuracy) > 0 && Accuracy < precision)
                            {
                                ImageIndicator = "sgreen.png";
                                IsEnableStopButton = true;
                            }
                        }
                    }
                }
                IsBusy = false;
                IsEnableStartButton = true;
                IsEnableStopButton = false;
                IsStoppedGPSSearch = false;
            }
        }

        private async void StopSearchGPS()
        {
            IsStoppedGPSSearch = true;
        }
    }
}
