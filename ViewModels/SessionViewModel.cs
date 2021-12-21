﻿using BrewUI.Data;
using BrewUI.EventModels;
using BrewUI.Items;
using BrewUI.Models;
using BrewUI.UserControls;
using Caliburn.Micro;
using LiveCharts;
using LiveCharts.Configurations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static BrewUI.Models.MyEnums;

namespace BrewUI.ViewModels
{
    public class SessionViewModel : Screen,
        IHandle<SerialReceivedEvent>,
        IHandle<SessionRunningEvent>,
        IHandle<CurrentProcessUpdatedEvent>,
        IHandle<AddingGrainsEvent>,
        IHandle<ConnectionEvent>,
        IHandle<RecipeOpened>
    {
        #region Variables

        private IEventAggregator _events;

        private bool continueTask;

        // Process tracking data
        private string _currentProcess;
        public string CurrentProcess
        {
            get { return _currentProcess; }
            set
            {
                _currentProcess = value;
                NotifyOfPropertyChange(() => CurrentProcess);
                _events.PublishOnUIThread(new CurrentProcessUpdatedEvent { currentProcess = CurrentProcess });
                _events.PublishOnUIThread(new DebugDataUpdatedEvent { index = "CP", stringValue = CurrentProcess });
            }
        }

        private string _currentStep;
        public string CurrentStep
        {
            get { return _currentStep; }
            set
            {
                _currentStep = value;
                NotifyOfPropertyChange(() => CurrentStep);
                _events.PublishOnUIThread(new DebugDataUpdatedEvent { index = "CS", stringValue = CurrentStep });
            }
        }

        private string _currentWork;
        public string CurrentWork
        {
            get { return _currentWork; }
            set 
            { 
                _currentWork = value;
                NotifyOfPropertyChange(() => CurrentWork);
            }
        }

        private bool _connected;
        public bool Connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                NotifyOfPropertyChange(() => Connected);
            }
        }

        private bool _mashRunning;
        public bool MashRunning
        {
            get { return _mashRunning; }
            set
            {
                _mashRunning = value;
                NotifyOfPropertyChange(() => MashRunning);
                _events.PublishOnUIThread(new DebugDataUpdatedEvent { index = "MR", boolValue = MashRunning });
            }
        }

        private bool _spargeRunning;
        public bool SpargeRunning
        {
            get { return _spargeRunning; }
            set
            {
                _spargeRunning = value;
                NotifyOfPropertyChange(() => SpargeRunning);
                _events.PublishOnUIThread(new DebugDataUpdatedEvent { index = "SPR", boolValue = SpargeRunning });
            }
        }

        private bool _spargeCanStart;

        public bool SpargeCanStart
        {
            get { return _spargeCanStart; }
            set
            {
                _spargeCanStart = value;
                NotifyOfPropertyChange(() => SpargeCanStart);
            }
        }

        private bool _boilRunning;
        public bool BoilRunning
        {
            get { return _boilRunning; }
            set
            {
                _boilRunning = value;
                NotifyOfPropertyChange(() => BoilRunning);
                _events.PublishOnUIThread(new DebugDataUpdatedEvent { index = "BR", boolValue = BoilRunning });
            }
        }

        public bool strikeWaterAdded { get; set; }

        private bool _sessionRunning;
        public bool SessionRunning
        {
            get { return _sessionRunning; }
            set
            {
                _sessionRunning = value;
                NotifyOfPropertyChange(() => SessionRunning);
                _events.PublishOnUIThread(new DebugDataUpdatedEvent { index = "SR", boolValue = SessionRunning });
            }
        }

        private bool _runCooldown;
        public bool RunCooldown
        {
            get { return _runCooldown; }
            set
            {
                _runCooldown = value;
                NotifyOfPropertyChange(() => RunCooldown);
            }
        }

        private bool _cooldownRunning;
        public bool CooldownRunning
        {
            get { return _cooldownRunning; }
            set { 
                _cooldownRunning = value;
                NotifyOfPropertyChange(() => CooldownRunning);
            }
        }

        private bool _mashFinished;
        public bool MashFinished
        {
            get { return _mashFinished; }
            set
            {
                _mashFinished = value;
                NotifyOfPropertyChange(() => MashFinished);
            }
        }

        private bool _spargeFinished;
        public bool SpargeFinished
        {
            get { return _spargeFinished; }
            set
            {
                _spargeFinished = value;
                NotifyOfPropertyChange(() => SpargeFinished);
            }
        }

        private bool _boilFinished;
        public bool BoilFinished
        {
            get { return _boilFinished; }
            set
            {
                _boilFinished = value;
                NotifyOfPropertyChange(() => BoilFinished);
            }
        }

        public bool StopCurrentProcess { get; set; }

        public bool PumpControllerRunning { get; set; }

        // Progress tracking
        private int _sessionProgress;
        public int SessionProgress
        {
            get { return _sessionProgress; }
            set
            {
                _sessionProgress = value;
                NotifyOfPropertyChange(() => SessionProgress);
            }
        }

        private TimeSpan _sessionDuration;
        public TimeSpan SessionDuration
        {
            get { return _sessionDuration; }
            set
            {
                _sessionDuration = value;
            }
        }

        public DispatcherTimer SessionProgressTimer { get; set; }

        public TimeSpan TotalSessionTime { get; set; }

        public TimeSpan ElapsedSessionTime { get; set; }

        public DateTime ProcessFinishedTime { get; set; }

        // Recipe data
        public BreweryRecipe currentRecipe { get; set; }

        private string _sessionName;
        public string SessionName
        {
            get { return _sessionName; }
            set
            {
                _sessionName = value;
                NotifyOfPropertyChange(() => SessionName);
            }
        }

        private string _brewMethod;
        public string BrewMethod
        {
            get
            {
                return _brewMethod;
            }
            set
            {
                _brewMethod = value;
                NotifyOfPropertyChange(() => BrewMethod);
            }
        }

        private string _beerStyle;
        public string BeerStyle
        {
            get { return _beerStyle; }
            set
            {
                _beerStyle = value;
                NotifyOfPropertyChange(() => BeerStyle);
            }
        }

        private double _batchSize;
        public double BatchSize
        {
            get { return _batchSize; }
            set
            {
                _batchSize = value;
                NotifyOfPropertyChange(() => BatchSize);
            }
        }

        private double _mashWater;
        public double MashWater
        {
            get { return _mashWater; }
            set {
                _mashWater = value;
                NotifyOfPropertyChange(() => MashWater); 
            }
        }

        private bool _mashWaterAdded;
        public bool MashWaterAdded
        {
            get { return _mashWaterAdded; }
            set { 
                _mashWaterAdded = value;
                NotifyOfPropertyChange(() => MashWaterAdded);
            }
        }

        private double _batchVolume;
        public double BatchVolume
        {
            get { return _batchVolume; }
            set {
                _batchVolume = value;
                NotifyOfPropertyChange(() => BatchVolume);
            }
        }

        private double _postBoilVolume;
        public double PostBoilVolume
        {
            get { return _postBoilVolume; }
            set {
                _postBoilVolume = value;
                NotifyOfPropertyChange(() => PostBoilVolume);
            }
        }

        private double _boilVolume;
        public double BoilVolume
        {
            get { return _boilVolume; }
            set {
                _boilVolume = value;
                NotifyOfPropertyChange(() => BoilVolume);
            }
        }


        private ObservableCollection<MashStep> _mashSteps;
        public ObservableCollection<MashStep> MashSteps
        {
            get { return _mashSteps; }
            set
            {
                _mashSteps = value;
                NotifyOfPropertyChange(() => MashSteps);
            }
        }

        private ObservableCollection<Grain> _grainList;
        public ObservableCollection<Grain> GrainList
        {
            get { return _grainList; }
            set
            {
                _grainList = value;
                NotifyOfPropertyChange(() => GrainList);
            }
        }

        private List<Hops> _hopsList;
        public List<Hops> HopsList
        {
            get { return _hopsList; }
            set
            {
                _hopsList = value;
                NotifyOfPropertyChange(() => HopsList);
            }
        }

        private ObservableCollection<BoilStep> _boilSteps;
        public ObservableCollection<BoilStep> BoilSteps
        {
            get { return _boilSteps; }
            set
            {
                _boilSteps = value;
                NotifyOfPropertyChange(() => BoilSteps);
            }
        }

        public SpargeStep spargeStep { get; set; }

        private bool _mashBasketLifted;
        public bool MashBaskedLifted
        {
            get { return _mashBasketLifted; }
            set {
                _mashBasketLifted = value;
                NotifyOfPropertyChange(() => MashBaskedLifted);
            }
        }

        private double _spargeWaterAmount;
        public double SpargeWaterAmount
        {
            get { return _spargeWaterAmount; }
            set
            {
                _spargeWaterAmount = value;
                NotifyOfPropertyChange(() => SpargeWaterAmount);
            }
        }

        public bool grainsAdded;

        private double _targetTemp;
        public double TargetTemp
        {
            get { return _targetTemp; }
            set
            {
                _targetTemp = value;
                NotifyOfPropertyChange(() => TargetTemp);
                SendToArduino('t', TargetTemp.ToString());
            }
        }

        private double _targetDuration;
        public double TargetDuration
        {
            get { return _targetDuration; }
            set { _targetDuration = value; }
        }

        private double _spargeTemp;
        public double SpargeTemp
        {
            get { return _spargeTemp; }
            set
            {
                _spargeTemp = value;
                NotifyOfPropertyChange(() => SpargeTemp);
            }
        }

        public TimeSpan spargeDur { get; set; }
        public DateTime spargeEndTime { get; set; }

        private int _spargeDur;
        public int SpargeDur
        {
            get { return _spargeDur; }
            set {
                _spargeDur = value;
                NotifyOfPropertyChange(() => SpargeDur);
            }
        }

        private string _spargeTimerText;
        public string SpargeTimerText
        {
            get { return _spargeTimerText; }
            set
            {
                _spargeTimerText = value;
                NotifyOfPropertyChange(() => SpargeTimerText);
            }
        }

        private DispatcherTimer SpargeTimer = new DispatcherTimer();

        public DispatcherTimer boilTimer { get; set; }

        private DateTime boilStartTime;
        private DateTime boilEndTime;

        private string _boilTimerText;
        public string BoilTimerText
        {
            get { return _boilTimerText; }
            set
            {
                _boilTimerText = value;
                NotifyOfPropertyChange(() => BoilTimerText);
            }
        }

        private string _mashStatus;
        public string MashStatus
        {
            get { return _mashStatus; }
            set
            {
                _mashStatus = value;
                NotifyOfPropertyChange(() => MashStatus);
                CurrentWork = MashStatus;
            }
        }

        private string _spargeStatus;
        public string SpargeStatus
        {
            get { return _spargeStatus; }
            set
            {
                _spargeStatus = value;
                NotifyOfPropertyChange(() => SpargeStatus);
                CurrentWork = SpargeStatus;
            }
        }

        private string _boilStatus;
        public string BoilStatus
        {
            get { return _boilStatus; }
            set
            {
                _boilStatus = value;
                NotifyOfPropertyChange(() => BoilStatus);
                CurrentWork = BoilStatus;
            }
        }

        private string _cooldownStatus;

        public string CooldownStatus
        {
            get { return _cooldownStatus; }
            set { 
                _cooldownStatus = value;
                NotifyOfPropertyChange(() => CooldownStatus);
                CurrentWork = CooldownStatus;
            }
        }

        private double _boilWaterAddition;

        public double BoilWaterAddition
        {
            get { return _boilWaterAddition; }
            set
            {
                _boilWaterAddition = value;
                NotifyOfPropertyChange(() => BoilWaterAddition);
            }
        }

        public int numberofSteps { get; set; }

        public int stepCount { get; set; }

        private double _cdTargetTemp;

        public double CDTargetTemp
        {
            get { return _cdTargetTemp; }
            set
            {
                _cdTargetTemp = value;
                NotifyOfPropertyChange(() => CDTargetTemp);
            }
        }

        // Measured data
        private double _currentTemp;
        public double CurrentTemp
        {
            get { return _currentTemp; }
            set
            {
                _currentTemp = value;
                NotifyOfPropertyChange(() => CurrentTemp);
            }
        }

        // Temperature chart
        private double _axisMax;
        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                NotifyOfPropertyChange(() => AxisMax);
            }
        }

        private double _axisMin;
        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                NotifyOfPropertyChange(() => AxisMin);
            }
        }

        public ChartValues<TemperatureMeasure> chartValues { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }
        public DateTime startTime { get; set; }
        private DateTime chartStartTime;

        #endregion

        public SessionViewModel(IEventAggregator events)
        {
            Connected = false;

            // Eventhandler
            _events = events;
            _events.Subscribe(this);

            // Session info
            _sessionName = "";
            SessionRunning = false;

            // Start chart
            InitializeChart();

            // Set start values
            MashFinished = false;
            SpargeCanStart = false;
            RunCooldown = false;
            MashWaterAdded = false;

            // Initialize recipe data
            GrainList = new ObservableCollection<Grain>();
            MashSteps = new ObservableCollection<MashStep>();
            HopsList = new List<Hops>();
            BoilSteps = new ObservableCollection<BoilStep>();
            currentRecipe = new BreweryRecipe();
            grainsAdded = false;

            StopCurrentProcess = false;
            PumpControllerRunning = false;
        }

        #region UI Methods
        public void BackButton()
        {
            if (!BoilFinished)
            {
                MessageBoxResult answer = MessageBox.Show("This will abort the current brew session. Continue?", "Warning", MessageBoxButton.YesNo);
                if (answer == MessageBoxResult.No)
                {
                    return;
                }
            }
            _events.PublishOnUIThread(new SessionRunningEvent { SessionRunning = false });
        }

        public async Task Mash()
        {
            //if (!MashWaterAdded)
            //{
            //    MessageBox.Show("Please add mash water before starting.");
            //    return;
            //}

            List<StepConfirmation> mashConfirmations = new List<StepConfirmation>();
            mashConfirmations.Add(new StepConfirmation { Name = "Mount mash basket in brewery" });
            mashConfirmations.Add(new StepConfirmation { Name = "Connect mash basket hose" });
            mashConfirmations.Add(new StepConfirmation { Name = "Add mash water", Content = $"{MashWater} L" });

            if(!CustomWindowHandler.LoadStepConfirmationWindow(mashConfirmations, "Prepare mash"))
            {
                MessageBox.Show("Mash preparations not verified. Mash aborted.");
                return;
            }

            CurrentProcess = "Mash";
            MashStatus = "Waiting";
            startTime = DateTime.Now;
            chartStartTime = startTime;

            #region Add water, grains and preheat
            double grainWeight = 0;
            foreach (Grain grain in GrainList)
            {
                grainWeight += grain.amount;
            }


            TimeSpan boilDuration = TimeSpan.Zero;

            foreach (Hops hops in HopsList)
            {
                if (hops.BoilTime > boilDuration)
                {
                    boilDuration = hops.BoilTime;
                }
            }

            //double waterAmount = Math.Round(Calculations.MashWater(grainWeight), 1);

            // Add strike water and heat to strike temp
            //if (!strikeWaterAdded)
            //{
            //    double strikeTemp = Math.Round(Calculations.StrikeTemp(MashWater, MashSteps[0].stepTemp, grainWeight), 1);
            //    //MessageBoxResult answer = MessageBox.Show(string.Format("Strike temperature calculated to {0}°C.\n\nAdd {1}L of water and press OK.", strikeTemp, waterAmount), "Add strike water", MessageBoxButton.OKCancel);
            //    MessageBoxResult answer = MessageBox.Show(string.Format("Strike temperature calculated to {0}°C.\n\nAdd {1}L of water and press OK.", strikeTemp, MashWater), "Add strike water", MessageBoxButton.OKCancel);

            //    if (answer == MessageBoxResult.Cancel)
            //    {
            //        MessageBoxResult messageBoxResult = MessageBox.Show("This will abort the mash. Do you want to proceed?", "Warning", MessageBoxButton.YesNo);
            //        if (messageBoxResult == MessageBoxResult.Yes)
            //        {
            //            CloseMash();
            //            return;
            //        }
            //    }
            //}
            //strikeWaterAdded = true;

            double strikeTemp = Math.Round(Calculations.StrikeTemp(MashWater, MashSteps[0].stepTemp, grainWeight), 1);

            startTime = DateTime.Now;
            continueTask = false;
            TargetTemp = strikeTemp;

            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(1));
            MashStatus = "Preheating";
            Task heatAndKeep = Task.Run(() => HeatAndKeepAsync(TimeSpan.Zero), source.Token);
            await heatAndKeep;

            MashStep ms = MashSteps[0]; // Create this to have something to give to MashStepCancelled
            
            while (!continueTask)
            {
                // Check if process is exited
                if (MashStepCancelled(ms))
                {
                    CloseMash();
                    return;
                }
                await Task.Delay(100);
            }

            continueTask = false;

            // Check again if mash is cancelled
            if (MashStepCancelled(ms))
            {
                CloseMash();
                return;
            }

            FileInteraction.PlaySound(Sound.Finished);

            // Add grains
            if (!grainsAdded)
            {
                List<StepConfirmation> stepConfirmations = new List<StepConfirmation>();
                foreach (Grain grain in GrainList)
                {
                    stepConfirmations.Add(new StepConfirmation { Name = grain.name, Content = $"{grain.amount}g" });
                }
                grainsAdded = CustomWindowHandler.LoadStepConfirmationWindow(stepConfirmations, "Add grains");

                if (!grainsAdded)
                {
                    MessageBox.Show("Grains not added. Mash aborted.");
                    CloseMash();
                    return;
                }
            }

            #endregion

            #region Perform mash steps
            startTime = DateTime.Now;

            foreach (MashStep item in MashSteps)
            {
                // Check if any steps are already finished. If so, exit step and go to next.
                if (item.Status == "Finished")
                {
                    continue;
                }

                item.startTime = DateTime.Now;
                TargetTemp = item.stepTemp;
                CurrentStep = item.stepName;

                if (item.Status == "Paused")
                {
                    TargetDuration = item.timeLeft.TotalMinutes;
                }
                else
                {
                    TargetDuration = item.stepDuration.TotalMinutes;
                }

                #region Preheat mashstep
                item.Status = "Preheating";
                MashStatus = item.Status;

                await HeatAndKeepAsync(TimeSpan.Zero);

                // Check again if mash is cancelled
                if (MashStepCancelled(item, "Waiting"))
                {
                    CloseMash();
                    return;
                }

                continueTask = false;
                #endregion

                // Keep temperature for desired duration
                item.Status = "Mashing";
                MashStatus = item.Status;
                item.startTime = DateTime.Now;
                item.endTime = item.startTime + TimeSpan.FromMinutes(TargetDuration);
                item.stepTimer.Start();

                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Run(() => HeatAndKeepAsync(item.endTime - item.startTime)));
                tasks.Add(Task.Run(() =>
                {
                    // Check if process is exited
                    if (!SessionRunning || CurrentProcess.Length < 1 || MashStepCancelled(item, "Waiting"))
                    {
                        foreach (MashStep step in MashSteps)
                        {
                            if (step.Status == "Mashing")
                            {
                                step.Status = "Paused";
                                MashStatus = "Paused";
                                step.stepTimer.Stop();
                                step.timeLeft = item.endTime.Subtract(DateTime.Now);
                            }
                        }
                        CloseMash();
                        return;
                    }
                    Task.Delay(100).Wait();
                }));

                await Task.WhenAll(tasks);

                if(CurrentProcess.Length < 1)
                {
                    return;
                }

                item.Status = "Finished";
                item.stepTimer.Stop();
                item.TimerText = item.stepDuration.ToString("hh\\:mm\\:ss");

                UpdateProgressionBar();
            }

            #endregion

            CurrentProcess = "";
            MashStatus = "";
            CurrentStep = "";
            TargetTemp = 0;
            FileInteraction.PlaySound(Sound.Finished);
            MessageBox.Show("Mash finished!");
            MashFinished = true;
            SpargeCanStart = true;

            //SessionProgressTimer.Stop();
            ProcessFinishedTime = DateTime.Now;
        }

        private void UpdateProgressionBar()
        {
            stepCount += 1;
            SessionProgress = 100 / numberofSteps * stepCount;
            SendToArduino('p', SessionProgress.ToString());
        }

        private bool MashStepCancelled(MashStep ms, String itemStatus = "")
        {
            if (!SessionRunning || MashRunning == false)
            {
                if (itemStatus != "")
                {
                    ms.Status = itemStatus;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public async void StartMash()
        {
            // Cancel mash
            if (MashRunning)
            {
                MessageBoxResult result = MessageBox.Show("This will abort the current mash process. Continue?", "Warning", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    CloseMash();
                }
                return;
            }

            // Check if mash was paused previously
            foreach (MashStep step in MashSteps)
            {
                if (step.Status == "Paused" | step.Status == "Finished")
                {
                    MessageBoxResult answer_continue = MessageBox.Show("The mash was previously paused. Press Yes to proceed with the paused mash, or press No to restart.", "Warning", MessageBoxButton.YesNoCancel);
                    if (answer_continue == MessageBoxResult.No)
                    {
                        foreach (MashStep item in MashSteps)
                        {
                            item.Status = "Waiting";
                            item.TimerText = step.stepDuration.ToString("hh\\:mm\\:ss");
                        }
                    }
                    else if (answer_continue == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
            }

            Mash();
        }

        public async void StartSparge()
        {
            // Initiate cancellation token for async tasks
            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(1));

            // Cancel sparge if ongoing
            if (SpargeRunning)
            {
                MessageBoxResult msganswer = MessageBox.Show("This will abort the ongoing sparge.\n\nContinue?", "Cancel sparge", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (msganswer == MessageBoxResult.Yes)
                {
                    CloseSparge();
                    return;
                }
            }

            SpargeRunning = true;

            // Start sparge
            CurrentProcess = "Sparge";

            spargeStep = currentRecipe.spargeStep;
            SpargeStatus = "Preheating";
            spargeDur = TimeSpan.FromMinutes(spargeStep.spargeDur);
            spargeEndTime = DateTime.Now + spargeDur;
            SpargeTemp = spargeStep.spargeTemp;

            TargetDuration = spargeDur.TotalMinutes;
            TargetTemp = SpargeTemp;

            // Preheat
            Task heatAndKeep = Task.Run(() => HeatAndKeepAsync(TimeSpan.Zero), source.Token);
            await heatAndKeep;

            while (!continueTask)
            {
                // Check if process is exited
                if (!SessionRunning || CurrentProcess.Length < 1)
                {
                    CloseSparge();
                    return;
                }
                await Task.Delay(100);
            }
            continueTask = false;

            FileInteraction.PlaySound(Sound.Finished);

            List<StepConfirmation> stepConfirmations = new List<StepConfirmation>();
            stepConfirmations.Add(new StepConfirmation { Name = "Lift up mash basket" });
            stepConfirmations.Add(new StepConfirmation { Name = "Ensure hose is connected to mash basket top" });

            
            if (!CustomWindowHandler.LoadStepConfirmationWindow(stepConfirmations, "Sparge preparations"))
            {
                MessageBox.Show("Sparge preparations not verified. Sparge aborted.");
                CloseSparge();
                return;
            }

            SpargeStatus = "Sparging";

            // Initialize sparge timer
            SpargeTimer.Interval = TimeSpan.FromSeconds(1);
            SpargeTimer.Tick += SpargeTimer_Tick;
            SpargeTimer.Start();

            SendToArduino('P', "1");
            Task heaterController = Task.Run(() => HeaterController(spargeDur, source.Token));
            await heaterController;

            while (!continueTask)
            {
                // Check if process is exited
                if (!SessionRunning || CurrentProcess.Length < 1)
                {
                    CloseSparge();
                    return;
                }
                await Task.Delay(100);
            }

            SendToArduino('P', "0");
            SpargeTimer.Stop();
            SpargeTimerText = TimeSpan.FromMinutes(TargetDuration).ToString("hh\\:mm\\:ss");
            SpargeStatus = "Finished";
            FileInteraction.PlaySound(Sound.Finished);
            MessageBox.Show("Sparge finished!");
            SpargeFinished = true;
            CurrentProcess = "";

            //SessionProgressTimer.Stop();
            UpdateProgressionBar();
            ProcessFinishedTime = DateTime.Now;
        }

        public async void StartBoil()
        {
            // Check if boil is running
            if (BoilRunning)
            {
                MessageBoxResult answer = MessageBox.Show("This will abort the ongoing boil.\n\nContinue?", "Caution", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (answer == MessageBoxResult.Yes)
                {
                    CloseBoil();
                    return;
                }
            }

            // Add water to reach correct boil amount

            foreach (Hops hops in HopsList)
            {
                if (hops.BoilTime.TotalMinutes > TargetDuration)
                {
                    TargetDuration = hops.BoilTime.TotalMinutes;
                }
            }

            double grainBill = 0;

            foreach (Grain grain in GrainList)
            {
                grainBill += grain.amount;
            }

            //BoilWaterAddition = Math.Round(Calculations.TotalWater(grainBill, BatchSize, TimeSpan.FromMinutes(TargetDuration)) - Calculations.MashWater(grainBill) - SpargeWaterAmount, 1);

            CurrentProcess = "Boil";
            chartStartTime = DateTime.Now;

            TargetTemp = 100;

            //MessageBoxResult messageBoxResult = MessageBox.Show($"Add {BoilWaterAddition} liter of water to the vort and press OK", "Add water", MessageBoxButton.OKCancel);

            //if (messageBoxResult != MessageBoxResult.OK)
            //{
            //    CloseBoil();
            //    return;
            //}

            // Preheat
            BoilStatus = "Preheating";
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(1));
            await Task.Run(() => HeaterController(TimeSpan.Zero, cts.Token));

            while (!continueTask)
            {
                // Check if process is exited
                if (!SessionRunning || CurrentProcess.Length < 1)
                {
                    CloseBoil();
                    return;
                }
                Task.Delay(100).Wait();
            }
            continueTask = false;
            SendToArduino('P', "0");

            #region Perform boil steps

            BoilStatus = "Boiling";

            DateTime now = DateTime.Now;
            boilStartTime = now;
            boilEndTime = boilStartTime + TimeSpan.FromMinutes(TargetDuration);
            TimeSpan timeLeft = TimeSpan.FromMinutes(TargetDuration);

            boilTimer = new DispatcherTimer();
            boilTimer.Interval = TimeSpan.FromSeconds(1);
            boilTimer.Tick += BoilTimer_Tick;
            boilTimer.Start();

            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(1));

            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => HeaterController(boilEndTime - boilStartTime, source.Token)));
            tasks.Add(Task.Run(() =>
            {
                while (now < boilEndTime)
                {
                    // Check if process is exited
                    if (!SessionRunning || CurrentProcess.Length < 1)
                    {
                        CloseBoil();
                        return;
                    }
                    Task.Delay(100).Wait();

                    now = DateTime.Now;
                    timeLeft = boilEndTime - now;

                    foreach (BoilStep boilStep in BoilSteps)
                    {

                        if (boilStep.boilTime >= timeLeft && boilStep.added == false)
                        {
                            StringBuilder tempStr = new StringBuilder();
                            foreach (Hops hops in boilStep.hopsList)
                            {
                                string tempLine = hops.Amount + "g " + hops.Name;
                                tempStr.AppendLine(tempLine);
                            }
                            FileInteraction.PlaySound(Sound.Finished);
                            MessageBox.Show("Add the following hops and press Ok.\n\n" + tempStr, "Add hops");
                            boilStep.added = true;
                            UpdateProgressionBar();
                        }
                    }
                    now = DateTime.Now;
                    Task.Delay(100).Wait();
                }
            }));

            await Task.WhenAll(tasks);

            bool finished = true;
            foreach (BoilStep boilStep in BoilSteps)
            {
                if (boilStep.added == false)
                {
                    finished = false;
                }
            }

            if (!finished)
            {
                MessageBox.Show("Boil aborted.");
                CloseBoil();
                return;
            }

            FileInteraction.PlaySound(Sound.Finished);
            BoilStatus = "Finished";
            MessageBox.Show("Boil has finished!");
            BoilFinished = true;
            CloseBoil();
            #endregion
        }

        public void StartCooldown()
        {
            if (CooldownRunning)
            {
                CloseCooldown();
            }
            else
            {
                Cooldown();
            }
        }

        private async Task Cooldown()
        {
            List<StepConfirmation> stepConfirmations = new List<StepConfirmation>();
            stepConfirmations.Add(new StepConfirmation { Name = "Remove hops from brewery" });
            stepConfirmations.Add(new StepConfirmation { Name = "Insert cooling coil and connect to water tap" });
            CustomWindowHandler.LoadStepConfirmationWindow(stepConfirmations, "Cooldown preparations");

            CooldownRunning = true;
            TargetTemp = currentRecipe.cooldownTargetTemp;
            CurrentProcess = "Cooldown";
            CurrentStep = "Cooling";

            while (TargetTemp < CurrentTemp)
            {
                if (!CooldownRunning)
                {
                    return;
                }
                await Task.Delay(1000);
            }

            MessageBox.Show("Cooldown temperature reached. Brew session finished.", "Cooldown temperature reached");
            CloseCooldown();
        }

        private void CloseMash()
        {
            CurrentProcess = "";
            CurrentStep = "";
            TargetTemp = 0;
            TargetDuration = 0;
            SendToArduino('H', "0");
            SendToArduino('P', "0");
            MashRunning = false;
            continueTask = false;
        }

        private void CloseSparge()
        {
            SpargeRunning = false;
            SpargeTimerText = TimeSpan.FromMinutes(TargetDuration).ToString("hh\\:mm\\:ss");
            SpargeStatus = "Waiting";
            CurrentProcess = "";
            SpargeTimer.Stop();
            TargetDuration = 0;
            SendToArduino('H', "0");
            SendToArduino('P', "0");
        }

        private void CloseBoil()
        {
            CurrentProcess = "";
            CurrentStep = "";
            BoilRunning = false;
            TargetTemp = 0;
            TargetDuration = 0;
            SendToArduino('H', "0");
            SendToArduino('P', "0");
            BoilStatus = "-";
            try
            {
                boilTimer.Stop();
            }
            catch
            {

            }
            continueTask = false;
        }

        private void CloseCooldown()
        {
            CooldownRunning = false;
            TargetTemp = 0;
            CurrentProcess = "";
        }

        private void BoilTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now <= boilEndTime)
            {
                BoilTimerText = (boilEndTime - DateTime.Now).ToString("hh\\:mm\\:ss");
            }
        }

        private void SpargeTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan timeLeft = spargeEndTime - DateTime.Now;
            SpargeTimerText = timeLeft.ToString("hh\\:mm\\:ss");
        }

        #endregion

        #region Methods

        private async Task HeatAndKeepAsync(TimeSpan duration)
        {
            _events.PublishOnUIThread(new DebugDataUpdatedEvent { index = "HAKA Started", stringValue = "" });
            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(1));

            List<Task> tasks = new List<Task>();

            tasks.Add(Task.Run(() => PumpController(duration, source.Token)));
            tasks.Add(Task.Run(() => HeaterController(duration, source.Token)));

            await Task.WhenAll(tasks);
            _events.PublishOnUIThread(new DebugDataUpdatedEvent { index = "HAKA Finished", stringValue = "" });
        }

        private void PumpController(TimeSpan duration, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.Now;
            bool pumpOn = false;
            bool sendMessage = false;
            DateTime pumpEnd = now + TimeSpan.FromSeconds(1);

            ArduinoMessage arduinoMessage = new ArduinoMessage();
            arduinoMessage.AIndex = 'P';

            // Preheating
            try
            {
                while (CurrentTemp <= TargetTemp - 0.5)
                {
                    // Check if session has been aborted
                    if (!SessionRunning || CurrentProcess.Length < 1)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    if (now >= pumpEnd)
                    {
                        sendMessage = true;
                    }

                    if (!pumpOn && sendMessage)
                    {
                        arduinoMessage.AMessage = "1";
                        _events.PublishOnUIThread(new SerialToSendEvent { arduinoMessage = arduinoMessage });
                        pumpEnd = now + TimeSpan.FromSeconds(Properties.Settings.Default.PumpOnDuration);
                        sendMessage = false;
                        pumpOn = !pumpOn;
                    }

                    if (pumpOn && sendMessage)
                    {
                        arduinoMessage.AMessage = "0";
                        _events.PublishOnUIThread(new SerialToSendEvent { arduinoMessage = arduinoMessage });
                        pumpEnd = now + TimeSpan.FromSeconds(Properties.Settings.Default.PumpOffDuration);
                        sendMessage = false;
                        pumpOn = !pumpOn;
                    }

                    now = DateTime.Now;
                    Task.Delay(300).Wait();
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            _events.PublishOnUIThread(new DebugDataUpdatedEvent { index = "DUR", stringValue = duration.TotalSeconds.ToString() });

            if (duration == TimeSpan.Zero)
            {
                return;
            }

            // Keeping temp for duration
            DateTime endTime = now + duration;
            try
            {
                while (now < endTime)
                {
                    // Check if session has been aborted
                    if (!SessionRunning || CurrentProcess.Length < 1)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    if (now >= pumpEnd)
                    {
                        sendMessage = true;
                    }

                    if (!pumpOn && sendMessage)
                    {
                        arduinoMessage.AMessage = "1";
                        _events.PublishOnUIThread(new SerialToSendEvent { arduinoMessage = arduinoMessage });
                        pumpEnd = now + TimeSpan.FromSeconds(Properties.Settings.Default.PumpOnDuration);
                        sendMessage = false;
                        pumpOn = !pumpOn;
                    }

                    if (pumpOn && sendMessage)
                    {
                        arduinoMessage.AMessage = "0";
                        _events.PublishOnUIThread(new SerialToSendEvent { arduinoMessage = arduinoMessage });
                        pumpEnd = now + TimeSpan.FromSeconds(Properties.Settings.Default.PumpOffDuration);
                        sendMessage = false;
                        pumpOn = !pumpOn;
                    }

                    now = DateTime.Now;
                    Task.Delay(300).Wait();
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            // Turn pump off when finished
            arduinoMessage.AMessage = "0";
            _events.PublishOnUIThread(new SerialToSendEvent { arduinoMessage = arduinoMessage });
        }

        private void HeaterController(TimeSpan duration, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.Now;
            bool sendMessage = true;
            bool heating = false;

            ArduinoMessage arduinoMessage = new ArduinoMessage();
            arduinoMessage.AIndex = 'H';

            // Preheating
            while (CurrentTemp <= TargetTemp - 0.5)
            {
                // Check if session has been aborted
                if (!SessionRunning || CurrentProcess.Length < 1)
                {
                    return;
                }

                if (sendMessage == false)
                {
                    if (heating == false && CurrentTemp < TargetTemp - 0.5) // Heater off and temp too low
                    {
                        sendMessage = true;
                    }
                    else if (heating == true && CurrentTemp >= TargetTemp - 0.5) // Heater on and temp reach target
                    {
                        sendMessage = true;
                    }
                }

                if (heating == true && sendMessage == true) // Turn heater off
                {
                    arduinoMessage.AMessage = "0";
                    _events.PublishOnUIThread(new SerialToSendEvent { arduinoMessage = arduinoMessage });
                    sendMessage = false;
                    heating = false;
                }
                else if (heating == false && sendMessage == true) // Turn heater on
                {
                    arduinoMessage.AMessage = "1";
                    _events.PublishOnUIThread(new SerialToSendEvent { arduinoMessage = arduinoMessage });
                    sendMessage = false;
                    heating = true;
                }

                now = DateTime.Now;
                Task.Delay(300).Wait();
            }

            if (duration == TimeSpan.Zero)
            {
                continueTask = true;
                return;
            }

            // Keep temp for duration
            now = DateTime.Now;
            DateTime endTime = now + duration;
            while (now < endTime)
            {
                // Check if session has been aborted
                if (!SessionRunning || CurrentProcess.Length < 1)
                {
                    return;
                }

                if (sendMessage == false)
                {
                    if (heating == false && CurrentTemp < TargetTemp - 0.5) // Heater off and temp too low
                    {
                        sendMessage = true;
                    }
                    else if (heating == true && CurrentTemp >= TargetTemp - 0.5) // Heater on and temp reach target
                    {
                        sendMessage = true;
                    }
                }

                if (heating == true && sendMessage == true) // Turn heater off
                {
                    arduinoMessage.AMessage = "0";
                    _events.PublishOnUIThread(new SerialToSendEvent { arduinoMessage = arduinoMessage });
                    sendMessage = false;
                    heating = false;
                }
                else if (heating == false && sendMessage == true) // Turn heater on
                {
                    arduinoMessage.AMessage = "1";
                    _events.PublishOnUIThread(new SerialToSendEvent { arduinoMessage = arduinoMessage });
                    sendMessage = false;
                    heating = true;
                }

                now = DateTime.Now;
                Task.Delay(300);
            }

            arduinoMessage.AMessage = "0";
            _events.PublishOnUIThread(new SerialToSendEvent { arduinoMessage = arduinoMessage });
            continueTask = true;
        }

        public void SendToArduino(char _index, string _message = "")
        {
            ArduinoMessage _arduinoMessage = new ArduinoMessage();
            _arduinoMessage.AIndex = _index;
            _arduinoMessage.AMessage = _message;
            Thread.Sleep(300);
            _events.PublishOnUIThread(new SerialToSendEvent { arduinoMessage = _arduinoMessage });
        }

        private void InitializeChart()
        {
            var mapper = Mappers.Xy<TemperatureMeasure>()
                .X(model => model.measureTime.Ticks)   //use measureTime.Ticks as X
                .Y(model => model.measureTemp);        //use the temp property as Y

            Charting.For<TemperatureMeasure>(mapper); // Save the mapper globally
            chartValues = new ChartValues<TemperatureMeasure>(); // chartValues will hold all measured values

            AxisStep = TimeSpan.FromMinutes(5).Ticks;
            AxisUnit = TimeSpan.TicksPerSecond;

            DateTimeFormatter = value => new TimeSpan((long)value).ToString(); // How to format the x-axis
            chartValues.Add(new TemperatureMeasure { measureTemp = 0, measureTime = TimeSpan.Zero });
        }

        private int SetStepProgressionShares()
        {
            int stepCount = 0;

            // Mash steps
            foreach (MashStep mashStep in MashSteps)
            {
                stepCount += 1;
            }

            // Sparge
            stepCount += 1;

            // Boil
            foreach (BoilStep bs in BoilSteps)
            {
                stepCount += 1;
            }

            return stepCount;
        }

        #endregion

        #region Event handlers

        public void Handle(SessionRunningEvent message)
        {
            if (message.SessionRunning == true)
            {
                SessionRunning = true;
                currentRecipe = message.BreweryRecipe;
                _brewMethod = message.BreweryRecipe.sessionInfo.BrewMethod;
                _sessionName = message.BreweryRecipe.sessionInfo.sessionName;
                _beerStyle = message.BreweryRecipe.sessionInfo.style.Name;
                _batchSize = message.BreweryRecipe.sessionInfo.BatchSize;
                MashSteps = message.BreweryRecipe.mashSteps;

                // Status for all processes
                MashStatus = "Waiting";
                SpargeStatus = "Waiting";
                BoilStatus = "Waiting";
                CooldownStatus = "Waiting";

                strikeWaterAdded = false;

                startTime = DateTime.Now;

                foreach (MashStep step in MashSteps)
                {
                    step.Status = "Waiting";
                }

                MashBaskedLifted = false;

                GrainList = message.BreweryRecipe.grainList;
                spargeStep = message.BreweryRecipe.spargeStep;
                SpargeWaterAmount = spargeStep.spargeWaterAmount;
                SpargeTemp = spargeStep.spargeTemp;
                SpargeDur = spargeStep.spargeDur;
                SpargeTimerText = TimeSpan.FromMinutes(SpargeDur).ToString("hh\\:mm\\:ss");

                // Retrieve hops list and create boil steps
                foreach (Hops hops in message.BreweryRecipe.hopsList)
                {
                    HopsList.Add(hops);
                }

                HopsList = HopsList.OrderByDescending(x => x.BoilTime).ToList();

                TimeSpan prevBoilTime = TimeSpan.Zero;
                int i = 0;

                foreach (Hops hops in HopsList)
                {
                    if (hops.BoilTime != prevBoilTime)
                    {
                        BoilSteps.Add(new BoilStep());
                        i = BoilSteps.Count - 1;
                        BoilSteps[i].boilTime = hops.BoilTime;
                        BoilSteps[i].hopsList = new ObservableCollection<Hops>();
                    }
                    BoilSteps[i].hopsList.Add(hops);
                    BoilSteps[i].added = false;
                    prevBoilTime = hops.BoilTime;
                }

                // Cooldown 
                CDTargetTemp = message.BreweryRecipe.cooldownTargetTemp;
                if (CDTargetTemp > 0)
                {
                    RunCooldown = true;
                }
                else
                {
                    RunCooldown = false;
                }
                numberofSteps = SetStepProgressionShares();

                // Step temperatures
                // Calculate water amounts for all steps
                BatchVolume = BatchSize + Properties.Settings.Default.FermentertLoss;
                PostBoilVolume = Calculations.PostBoilVolume(BatchVolume);
                TimeSpan totalBoilTime = TimeSpan.FromSeconds(0);
                foreach (Hops hops in HopsList)
                {
                    if (hops.BoilTime > totalBoilTime)
                    {
                        totalBoilTime = hops.BoilTime;
                    }
                }
                BoilVolume = Calculations.BoilVolume(PostBoilVolume, totalBoilTime.TotalMinutes, Properties.Settings.Default.EvaporationRate, Properties.Settings.Default.BoilLoss);
                double grainBill = 0;
                foreach (Grain grain in GrainList)
                {
                    grainBill += grain.amount;
                }

                MashWater = Math.Round(Calculations.TotalWater(BoilVolume, grainBill),1);
            }
            else
            {
                SessionRunning = false;
                MashRunning = false;
                SpargeRunning = false;
                BoilRunning = false;
                MashFinished = false;
                SpargeFinished = false;
                BoilFinished = false;
                grainsAdded = false;
                strikeWaterAdded = false;
                BoilSteps.Clear();
                HopsList.Clear();
                MashSteps.Clear();
                chartValues.Clear();
                GrainList.Clear();
                SendToArduino('H', "0");
                SendToArduino('P', "0");
                if (boilTimer != null)
                {
                    boilTimer.Stop();
                }
                SessionProgress = 0;
            }
        }

        public void Handle(SerialReceivedEvent message)
        {
            char _index = message.arduinoMessage.AIndex;
            string _value = message.arduinoMessage.AMessage;

            // Handle incomming data
            switch (_index)
            {
                case 'T':
                    CurrentTemp = Calculations.StringToDouble(_value);
                    chartValues.Add(new TemperatureMeasure { measureTemp = CurrentTemp, measureTime = DateTime.Now.Subtract(chartStartTime) });
                    break;
                default:

                    break;
            }
        }

        public void Handle(CurrentProcessUpdatedEvent processName)
        {
            switch (processName.currentProcess)
            {
                case "Mash":
                    MashRunning = true;
                    SpargeRunning = false;
                    BoilRunning = false;
                    CooldownRunning = false;
                    break;
                case "Sparge":
                    MashRunning = false;
                    SpargeRunning = true;
                    BoilRunning = false;
                    CooldownRunning = false;
                    break;
                case "Boil":
                    MashRunning = false;
                    SpargeRunning = false;
                    BoilRunning = true;
                    CooldownRunning = false;
                    break;
                case "Cooldown":
                    MashRunning = false;
                    SpargeRunning = false;
                    BoilRunning = false;
                    CooldownRunning = true;
                    break;
                default:
                    MashRunning = false;
                    SpargeRunning = false;
                    BoilRunning = false;
                    CooldownRunning = false;
                    break;
            }
            chartValues.Clear();
        }

        public void Handle(AddingGrainsEvent message)
        {
            grainsAdded = message.grainsAdded;
        }

        public void Handle(ConnectionEvent message)
        {
            if (message.ConnectionStatus == MyEnums.ConnectionStatus.Connected)
            {
                Connected = true;
            }
            else
            {
                Connected = false;
            }
        }

        public void Handle(RecipeOpened message)
        {
            _events.PublishOnUIThread(new SessionRunningEvent { SessionRunning = false });
        }


        #endregion
    }
}
