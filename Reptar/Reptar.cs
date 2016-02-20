////////////////////////////////////////////////////////////////////////////////// 
// Reptar Beacon Following                                                      //
//                                                                              //
// Copyright 2015                                                               //
// SmallRobots.it                                                               //
//                                                                              //
// This code is happily shared under                                            //
// The Code Project Open License (CPOL) 1.02                                    //
//                                                                              //
// THIS WORK IS PROVIDED "AS IS", "WHERE IS" AND "AS AVAILABLE",                //
// WITHOUT ANY EXPRESS OR IMPLIED WARRANTIES OR CONDITIONS OR GUARANTEES.       //
// YOU, THE USER, ASSUME ALL RISK IN ITS USE, INCLUDING COPYRIGHT INFRINGEMENT, //
// PATENT INFRINGEMENT, SUITABILITY, ETC. AUTHOR EXPRESSLY DISCLAIMS ALL        //
// EXPRESS, IMPLIED OR STATUTORY WARRANTIES OR CONDITIONS, INCLUDING WITHOUT    // 
// LIMITATION, WARRANTIES OR CONDITIONS OF MERCHANTABILITY, MERCHANTABLE        //
// QUALITY OR FITNESS FOR A PARTICULAR PURPOSE, OR ANY WARRANTY OF TITLE        //
// OR NON-INFRINGEMENT, OR THAT THE WORK (OR ANY PORTION THEREOF) IS CORRECT,   //
// USEFUL, BUG-FREE OR FREE OF VIRUSES. YOU MUST PASS THIS DISCLAIMER ON        //
// WHENEVER YOU DISTRIBUTE THE WORK OR DERIVATIVE WORKS                         //
//////////////////////////////////////////////////////////////////////////////////

using System;
using MonoBrickFirmware;
using MonoBrickFirmware.Display.Dialogs;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using System.Threading;
using MonoBrickFirmware.UserInput;
using SmallRobots.Controllers;

namespace SmallRobots.Reptar
{
	/// <summary>
	/// Reptar.
	/// </summary>
	public partial class Reptar
	{
		#region Fields		
		// Wave thread
		// This thread generates a sinusoidal wave
		EventWaitHandle stopWaveThread;
		int waveThreadSampleTime;
		Thread waveThread;
		double amplitude;	// Wave apmplitude
		double meanValue;	// Wave mean value
		double frequency;   // Frequency expressed in Hz
		double wave;		// This is the output of the thread
        bool waveEnabled;
        sbyte biteSetpoint;

		// Mission Control Thread
		// This thread changes the behaviour of the Reptar
		// based on the actual phase of the mission
		EventWaitHandle stopMissionControlThread;
		int missonControlThreadSampleTime;
		Thread missionControlThread;

		// LCD Update Thread
		// This thred updates the LCD during execution
		EventWaitHandle stopLCDThread;
		int lCDThreadSampleTime;
		Thread lCDThread;

		// IO UpdateThread
		EventWaitHandle stopIOUpdate; 
		int ioUpdateSamplingTime; 
		Thread ioUpdateThread;

		// Reptar oscillating motor
		sbyte currentSnakeAngle;
		Motor reptarAngleMotor;
		MediumMotorPositionPID reptarAngleRegulator;

		// Reptar main motor
		Motor reptarMainMotor;

		// Reptar neck and terrible mouth motor
		Motor reptarNeckMotor;

		// Reptar IR Sensor
		EV3IRSensor irSensor;
		sbyte beaconHeading;
		sbyte beaconDistance;

// Mission phases definition
enum MissionPhase
{
	Roam = 0,
	Chase,
	Bite
};

// Mission phases
MissionPhase currentMissionPhase;
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public Reptar ()
		{
			// IO
			reptarAngleMotor = new Motor(MotorPort.OutA);
			reptarMainMotor = new Motor (MotorPort.OutB);
			reptarNeckMotor = new Motor (MotorPort.OutC);

			// Mission Control Thread initialization
			stopMissionControlThread = new ManualResetEvent(false);
			missonControlThreadSampleTime = 100;
			missionControlThread = new Thread (MissionControlThread);

			// Wave thread initialization
			stopWaveThread = new ManualResetEvent(false);
			waveThreadSampleTime = 50;
			waveThread = new Thread (WaveThread);
			amplitude = 50;
			frequency = 0.5;
			meanValue = 0.0;

			// LCD Thread initializaztion
			stopLCDThread = new ManualResetEvent (false);
			lCDThreadSampleTime = 250;
			lCDThread = new Thread (LCDThread);

			// IO update thread
			stopIOUpdate = new ManualResetEvent(false);
			ioUpdateSamplingTime = 10;
			ioUpdateThread = new Thread (IOUpdateThread);

			// Reptar oscillating motor
			reptarAngleRegulator = new MediumMotorPositionPID ();
			reptarAngleRegulator.Motor = reptarAngleMotor;
			reptarAngleRegulator.SampleTime = 10;
			reptarAngleRegulator.MaxPower = (sbyte) 100;
			reptarAngleRegulator.MinPower = (sbyte) -100;
			reptarAngleRegulator.Kp = 3f;
			reptarAngleRegulator.Ki = 0.2f;

			// IR Sensor
			irSensor = new EV3IRSensor(SensorPort.In4);

			// Mission phases
			currentMissionPhase = MissionPhase.Roam;
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Start this instance
		/// </summary>
		public void Start()
		{
			// Program stop event
			ManualResetEvent terminateProgram = new ManualResetEvent(false);

			// Welcome messages
			LcdConsole.Clear();
			LcdConsole.WriteLine ("*****************************");
			LcdConsole.WriteLine ("*                           *");
			LcdConsole.WriteLine ("*      SmallRobots.it       *");
			LcdConsole.WriteLine ("*                           *");
			LcdConsole.WriteLine ("*        Reptar 1.0         *");
			LcdConsole.WriteLine ("*                           *");
			LcdConsole.WriteLine ("*                           *");
			LcdConsole.WriteLine ("*  Press Enter to start     *");
			LcdConsole.WriteLine ("*  Press Esc to terminate   *");
			LcdConsole.WriteLine ("*                           *");
			LcdConsole.WriteLine ("*****************************");

			// Button events
			ButtonEvents buttonEvents = new ButtonEvents ();

			// Enter button
			buttonEvents.EnterPressed += () => 
			{  
				// Message application starting
				LcdConsole.Clear();
				LcdConsole.WriteLine ("*****************************");
				LcdConsole.WriteLine ("*                           *");
				LcdConsole.WriteLine ("*      SmallRobots.it       *");
				LcdConsole.WriteLine ("*                           *");
				LcdConsole.WriteLine ("*        Reptar 1.0         *");
				LcdConsole.WriteLine ("*                           *");
				LcdConsole.WriteLine ("*                           *");
				LcdConsole.WriteLine ("*                           *");
				LcdConsole.WriteLine ("*                           *");
				LcdConsole.WriteLine ("* Starting...               *");
				LcdConsole.WriteLine ("*****************************");	

				// Starts the threads
				waveThread.Start();
				ioUpdateThread.Start();
				reptarAngleRegulator.Start();
				lCDThread.Start();
				missionControlThread.Start();
			}; 

			// Right button
			buttonEvents.RightPressed += () => 
			{

			};

			// Left button
			buttonEvents.LeftPressed += () => 
			{

			};

			// Escape button
			buttonEvents.EscapePressed += () => 
			{  
				// Terminates wave generation thread
				stopLCDThread.Set();
				if (lCDThread.IsAlive)
				{
					// Wait for termination
					lCDThread.Join();
				}
				LcdConsole.Clear();
				LcdConsole.WriteLine("LCD thread thread terminated.");

				// Terminates the snake angle regulator
				reptarAngleRegulator.Stop();
				LcdConsole.WriteLine("Snake angle regulator terminated.");

				// Terminates the IO Update Thread
				stopIOUpdate.Set();
				if (ioUpdateThread.IsAlive)
				{
					// Wait for termination
					ioUpdateThread.Join();
				}
				LcdConsole.WriteLine("IO update thread thread terminated.");

				// Terminates wave generation thread
				stopWaveThread.Set();
				if (waveThread.IsAlive)
				{
					// Wait for termination
					waveThread.Join();
				}
				LcdConsole.WriteLine("Wave generation thread thread terminated.");

				// Terminates the mission control thread
				stopMissionControlThread.Set();
				if (missionControlThread.IsAlive)
				{
					// Wait for termination
					missionControlThread.Join();
				}

				// Message application terminating
				LcdConsole.Clear();
				LcdConsole.WriteLine ("*****************************");
				LcdConsole.WriteLine ("*                           *");
				LcdConsole.WriteLine ("*      SmallRobots.it       *");
				LcdConsole.WriteLine ("*                           *");
				LcdConsole.WriteLine ("*        Reptar 1.0         *");
				LcdConsole.WriteLine ("*                           *");
				LcdConsole.WriteLine ("*                           *");
				LcdConsole.WriteLine ("*                           *");
				LcdConsole.WriteLine ("*                           *");
				LcdConsole.WriteLine ("* Terminating...            *");
				LcdConsole.WriteLine ("*****************************");	

				// Shut off all motors
				reptarMainMotor.Off();
				reptarAngleMotor.Off();
				reptarNeckMotor.Off();

				// Switch off the leds
				Buttons.LedPattern(0);

				// Wait a second
				Thread.Sleep(1000);

				// And terminates
				terminateProgram.Set(); 
			}; 

			// Wait for termination
			// The termination is set when user press the "Escape" button
			terminateProgram.WaitOne ();
			buttonEvents.Dispose();
		}

		#endregion

		#region Thread definitions
		/// <summary>
		/// LCD Thread
		/// </summary>
		void LCDThread()
		{
			Thread.CurrentThread.IsBackground = true;
			Font f = Font.MediumFont;
			Point offset = new Point(0,25);
			Point offset2 = new Point (0, 50);
			Point offset3 = new Point (0, 75);
			Point p = new Point(10, Lcd.Height - 125);
			Point boxSize = new Point(100, 24);
			Rectangle box = new Rectangle(p, p+boxSize);

			while (!stopLCDThread.WaitOne(lCDThreadSampleTime))
			{
				Lcd.Clear (); 
				Lcd.WriteTextBox (f, box , 			"Wave = " + wave.ToString("F2"), true); 
				Lcd.WriteTextBox (f, box + offset , "Mean = " + meanValue.ToString("F2"), true); 
				Lcd.WriteTextBox (f, box + offset2 , "Heading = " + beaconHeading.ToString("F2"), true); 
				Lcd.WriteTextBox (f, box + offset3 , "Distance = " + beaconDistance.ToString("F2"), true); 
				Lcd.Update (); 
			}
		}

        /// <summary>
        /// Generates a sinusoidal wave
        /// </summary>
        void WaveThread()
        {
        	UInt64 k = 0;										// Discrete time counter	
        	double t = 0;										// Time	
        	double angularPulsation = 2 * Math.PI * frequency; 	// Angular pulsation

        	while (!stopWaveThread.WaitOne(waveThreadSampleTime))
        	{
        		// Current time
        		t = ((double) k * waveThreadSampleTime) / 1000;

        		// Wave signal computation
        		wave = meanValue + amplitude * Math.Sin (angularPulsation * t);

        		// Update next discrete time istant
        		if (k < UInt64.MaxValue) {
        			k++;
        		} else {
        			k = 0;
        		}
        	}
        }

        /// <summary>
        /// IO update thread
        /// </summary>
        void IOUpdateThread()
        {
        	Thread.CurrentThread.IsBackground = true;

        	while (!stopIOUpdate.WaitOne (ioUpdateSamplingTime)) 
        	{
        		// Current snake angle and feeding to the oscillating Thread
        		currentSnakeAngle = (sbyte) reptarAngleMotor.GetTachoCount();
        		reptarAngleRegulator.InputSignal = (sbyte) currentSnakeAngle;

                if (waveEnabled)
                {
                    reptarAngleRegulator.SetPoint = (sbyte)wave;
                }
                else
                {
                    reptarAngleRegulator.SetPoint = biteSetpoint;
                }
        		reptarAngleMotor.SetPower (reptarAngleRegulator.OutputSignal);

        		// Updates the beacon heading only when the head is centered
                if (((currentSnakeAngle > -3) && (currentSnakeAngle < 3)) || !waveEnabled)
        		{
        			BeaconLocation beaconLocation = irSensor.ReadBeaconLocation ();
        			beaconHeading = (sbyte) beaconLocation.Location;
        			beaconDistance = (sbyte) beaconLocation.Distance;
        		}
        	}
        }

        /// <summary>
        /// Missions the control thread
        /// </summary>
        void MissionControlThread()
        {
        	Thread.CurrentThread.IsBackground = true;
            bool bitten = false;
        	while (!stopMissionControlThread.WaitOne (missonControlThreadSampleTime))
        	{
        		// Next phase will be the same if not transitioned
        		MissionPhase nextPhase = currentMissionPhase;
        		switch (currentMissionPhase)
        		{
        			case MissionPhase.Roam:
        				// Set main motor speed
        				reptarMainMotor.SetSpeed (50);
                        // Set led (led switched off);
        				Buttons.LedPattern (0);
                        // Oscillation 
                        waveEnabled = true;
        				// Transition
        				if (beaconDistance > 0)
        				{
        					nextPhase = MissionPhase.Chase;
        				} 
        				break;

        			case MissionPhase.Chase:
        				// Set main motor speed
        				reptarMainMotor.SetSpeed(30);
        				// Chasing
        				meanValue = 1.5 * beaconHeading;
                        // Set led (green);
        				Buttons.LedPattern(1);
                        // Oscillation 
                        waveEnabled = true;
        				// Next phase update
        				if (beaconDistance <= 0)
        				{
        					nextPhase = MissionPhase.Roam;
        				}
        				else if ((beaconDistance > 0) && (beaconDistance < 15))
        				{
        					nextPhase = MissionPhase.Bite;
        				}
        				break;

                    case MissionPhase.Bite:                      
                        // Set led (red);
                        Buttons.LedPattern(2);
                        // Oscillation 
                        waveEnabled = false;
                        biteSetpoint = beaconHeading;
                        // Bite
                        if (!bitten)
                        {
                            bitten = true;
                            GiveThreeBites();
                        }
                        // Next phase update
                        if (beaconDistance <= 0)
                        {
                            nextPhase = MissionPhase.Roam;
                            bitten = false;
                        }
        				break;
        		}
        		// Mission phase update
        		currentMissionPhase = nextPhase;
        	}
        }
		#endregion

        #region Private methods
        /// <summary>
        /// Gives three bites!
        /// </summary>
        void GiveThreeBites()
        {
            reptarMainMotor.Off();
            reptarMainMotor.SetSpeed(100);
            reptarNeckMotor.SpeedProfileTime(100, 100, 100, 100, true);
            Thread.Sleep(300);
            reptarNeckMotor.SpeedProfileTime((sbyte)-50, 100, 100, 100, true);
            Thread.Sleep(300);
            reptarNeckMotor.SpeedProfileTime(100, 100, 100, 100, true);
            Thread.Sleep(300);
            reptarNeckMotor.SpeedProfileTime((sbyte)-50, 100, 100, 100, true);
            Thread.Sleep(300);
            reptarNeckMotor.SpeedProfileTime(100, 100, 100, 100, true);
            Thread.Sleep(300);
            reptarNeckMotor.SpeedProfileTime((sbyte)-50, 100, 100, 100, true);
            reptarMainMotor.Off();
        }
        #endregion
	}
}

