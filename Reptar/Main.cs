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
using System.Threading;
using MonoBrickFirmware.Display.Menus;
using SmallRobots.Menus;

namespace SmallRobots.Reptar
{
	/// <summary>
	/// Main class
	/// </summary>
	class MainClass
	{
		#region Static Fields
		private static int previousNeckValue = 0;
		private static int previousHeadValue = 0;
		#endregion

		static public MenuContainer container;

		public static void Main (string[] args)
		{
			Menu menu = new Menu ("Reptar");
			container = new MenuContainer (menu);
			// menu.ParentMenuContainer = container;

			menu.AddItem(new ItemWithNumericInput("Calibrate Neck", 0, CalibrateNeck, -30, 30));
			menu.AddItem(new ItemWithNumericInput("Calibrate Head", 0, CalibrateHead, -30, 30));
			menu.AddItem (new MainMenuItem ("Start", Start_OnEnterPressed));
			menu.AddItem (new MainMenuItem ("Quit", Quit_OnEnterPressed));

			container.Show ();
		}

		public static void TerminateMenu()
		{
			container.Terminate ();
		}

		public static void CalibrateNeck (int newValue)
		{
			sbyte maxSpeed = 10;
			sbyte speed = 0;
            Motor Motor = new Motor (MotorPort.OutA);

			if (newValue > previousNeckValue)
			{
				speed = maxSpeed;
			} else
			{
				speed = (sbyte)-maxSpeed;
			}
			previousNeckValue = newValue;

			Motor.SpeedProfileTime (speed,100,100,100,true);
		}

		public static void CalibrateHead (int newValue)
		{
			sbyte maxSpeed = 10;
			sbyte speed = 0;
            Motor Motor = new Motor (MotorPort.OutC);

			if (newValue > previousHeadValue)
			{
				speed = (sbyte) - maxSpeed;
			} else
			{
				speed = (sbyte) maxSpeed;
			}
			previousHeadValue = newValue;

			Motor.SpeedProfileTime (speed, 100, 100, 100, true);		
		}

		public static void Start_OnEnterPressed()
		{
			container.SuspendButtonEvents ();
			 Reptar reptar = new Reptar ();
			 reptar.Start ();
			container.ResumeButtonEvents ();
		}

		public static void Quit_OnEnterPressed()
		{
			LcdConsole.Clear ();
			LcdConsole.WriteLine ("Terminating");
			// Wait a bit
			Thread.Sleep(1000);
			TerminateMenu ();
		}
	}
}

