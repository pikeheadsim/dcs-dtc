﻿using DTC.Models.F16.Radios;
using DTC.UI.Base;
using DTC.UI.Base.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DTC.UI.F16
{
	public partial class RadioPage : FeaturePage
	{
		private RadioSystem _radios;

		public RadioPage(RadioSystem radios, DataChanged dataChangedCallback) : base(dataChangedCallback)
		{
			_radios = radios;

			InitializeComponent();

			var tblComm1 = PopulateTable(_radios.COM1);
			var tblComm2 = PopulateTable(_radios.COM2);

			tblRadios.Controls.Add(tblComm1, 0, 1);
			tblRadios.Controls.Add(tblComm2, 1, 1);
		}

		private TableLayoutPanel PopulateTable(Radio radio)
		{
			var tblRadio = new TableLayoutPanel();
			tblRadio.RowCount = radio.Channels.Length;
			tblRadio.ColumnCount = 2;
			tblRadio.Dock = DockStyle.Fill;
			tblRadio.AutoScroll = true;
			var rowHeight = 30;

			for (int i = 0; i < radio.Channels.Length; i++)
			{
				RadioChannel channel = (RadioChannel)radio.Channels[i];

				var lbl = new Label();
				lbl.Text = "Channel " + channel.Channel.ToString();
				lbl.AutoSize = false;
				lbl.Dock = DockStyle.Fill;
				lbl.TextAlign = ContentAlignment.MiddleLeft;
				lbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10);
				tblRadio.Controls.Add(lbl, 0, i);

				var txt = new DTCTextBox();
				txt.Anchor = AnchorStyles.Left;
				txt.Font = new System.Drawing.Font("Microsoft Sans Serif", 10);
				txt.Mask = @"900\.00";
				txt.Text = String.Format(System.Globalization.CultureInfo.InvariantCulture, @"{0:0.00}", channel.Frequency);
				txt.Tag = channel;
				txt.GotFocus += Txt_GotFocus;
				txt.LostFocus += Txt_LostFocus;
				tblRadio.Controls.Add(txt, 1, i);

				tblRadio.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
			}

			return tblRadio;
		}

		private void Txt_GotFocus(object sender, EventArgs e)
		{
			var txt = ((DTCTextBox)sender);
			txt.SelectAll();
		}

		private void Txt_LostFocus(object sender, EventArgs e)
		{
			var txt = ((DTCTextBox)sender);
			var channel = (RadioChannel)txt.Tag;
			txt.Text = channel.SetFrequency(txt.Text);
			DataChangedCallback();
		}
	}
}
