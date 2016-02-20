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

namespace SmallRobots.Menus
{
	/// <summary>
	/// MenuItem that can be added to the MonoBrickFirmware.Display.Menus.Menu Class
	/// It features a delegate that can be istantiated to provide a callback for the Enter key pressed event
	/// </summary>
	public class MainMenuItem : ChildItemWithParent
	{
		/// <summary>
		/// Delegate for the OnEnterPressed() Event
		/// </summary>
		public delegate void RedefinedOnEnterPressed ();

		/// <summary>
		/// Delegate handler for the OnEnterPressed() Event
		/// </summary>
		public RedefinedOnEnterPressed RedefinedOnEnterPressedHandler;

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="SmallRobots.Menus.MainMenuItem"/> class.
		/// </summary>
		/// <param name="mainMenuItem">String for the menuItem</param>
		/// <param name="OnEnterPressed">Delegate handler for the OnEnterPressed() Event.</param>
		public MainMenuItem (string menuItem, RedefinedOnEnterPressed OnEnterPressed ) : base(menuItem)
		{
			RedefinedOnEnterPressedHandler = OnEnterPressed;
		}
		#endregion

		#region Public Methods
		public override void OnEnterPressed ()
		{
			base.OnEnterPressed ();
			if (RedefinedOnEnterPressedHandler != null)
			{
				RedefinedOnEnterPressedHandler ();
			}
		}
		#endregion
	}
}

