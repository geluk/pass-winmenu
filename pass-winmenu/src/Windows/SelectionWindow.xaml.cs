using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using PassWinmenu.Configuration;
using PassWinmenu.Hotkeys;
using PassWinmenu.Utilities;

namespace PassWinmenu.Windows
{
	/// <summary>
	/// Interaction logic for SelectionWindow.xaml
	/// </summary>
	internal abstract partial class SelectionWindow
	{
		private readonly int scrollBoundary;
		private readonly StyleConfig styleConfig;
		private int scrollOffset;
		private bool isClosing;
		private bool tryRemainOnTop = true;
		private bool firstActivation = true;
		private List<string> optionStrings = new List<string>();

		protected readonly List<SelectionLabel> Options = new List<SelectionLabel>();
		/// <summary>
		/// The label that is currently selected.
		/// </summary>
		public SelectionLabel Selected { get; protected set; }
		/// <summary>
		/// True if the user has chosen one of the options, false otherwise.
		/// </summary>
		public bool Success { get; protected set; }

		// TODO: either use this or remove it
		public string SearchHint { get; set; } = "Search...";

		/// <summary>
		/// Initialises the window with the provided options.
		/// </summary>
		protected SelectionWindow(SelectionWindowConfiguration configuration)
		{
			TimerHelper.Current.TakeSnapshot("mainwnd-creating");
			styleConfig = ConfigManager.Config.Interface.Style;
			scrollBoundary = ConfigManager.Config.Interface.Style.ScrollBoundary;

			// Position and size the window according to user configuration.
			Matrix fromDevice;
			using (var source = new HwndSource(new HwndSourceParameters()))
			{
				if (source.CompositionTarget == null)
				{
					// I doubt this path is ever triggered, but we'll handle it just in case.
					Log.Send("Could not determine the composition target. Window may not be positioned and sized correctly.", LogLevel.Warning);
					// We'll just use the identity matrix here.
					// This works fine as long as the screen's DPI scaling is set to 100%.
					fromDevice = Matrix.Identity;
				}
				else
				{
					fromDevice = source.CompositionTarget.TransformFromDevice;
				}
			}

			var position = fromDevice.Transform(configuration.Position);
			Left = position.X;
			Top = position.Y;

			var dimensions = fromDevice.Transform(configuration.Dimensions);
			Width = dimensions.X;
			MaxHeight = dimensions.Y;

			InitializeComponent();

			InitialiseLabels(configuration.Orientation);

			SearchBox.BorderBrush = styleConfig.Search.BorderColour;
			SearchBox.CaretBrush = styleConfig.CaretColour;
			SearchBox.Background = styleConfig.Search.BackgroundColour;
			SearchBox.Foreground = styleConfig.Search.TextColour;
			SearchBox.Margin = styleConfig.Search.Margin;
			SearchBox.BorderThickness = styleConfig.Search.BorderWidth;
			SearchBox.FontSize = styleConfig.FontSize;
			SearchBox.FontFamily = new FontFamily(styleConfig.FontFamily);

			Background = styleConfig.BackgroundColour;
			BorderBrush = styleConfig.BorderColour;
			BorderThickness = styleConfig.BorderWidth;

			TimerHelper.Current.TakeSnapshot("mainwnd-created");
		}


		private void InitialiseLabels(Orientation orientation)
		{
			var labelCount = 10;
			if (orientation == Orientation.Horizontal)
			{
				DockPanel.SetDock(SearchBox, Dock.Left);
				OptionsPanel.Orientation = Orientation.Horizontal;
			}
			else
			{
				// First measure how high the search box wants to be.
				SearchBox.Measure(new Size(double.MaxValue, double.MaxValue));
				// Now find out how much space we have to lay out our labels.
				var availableSpace = MaxHeight // Start with the maximum window height
				                   - Padding.Top - Padding.Bottom // Subtract window padding
				                   - WindowDock.Margin.Top - WindowDock.Margin.Bottom // Subtract window dock margin
				                   - SearchBox.DesiredSize.Height // Subtract size of the search box (includes margins)
				                   - OptionsPanel.Margin.Top - OptionsPanel.Margin.Bottom; // Subtract the margins of the options panel

				var labelHeight = CalculateLabelHeight();

				var labelFit = availableSpace / labelHeight;
				labelCount = (int)labelFit;

				if (!styleConfig.ScaleToFit)
				{
					var remainder = (labelFit - labelCount) * labelHeight;
					Log.Send($"Max height: {MaxHeight:F}, Available for labels: {availableSpace:F}, Total used by labels: {labelCount * labelHeight:F}, Remainder: {remainder:F}");
					//MinHeight = MaxHeight;
				}
			}

			for (var i = 0; i < labelCount; i++)
			{
				var label = CreateLabel($"label_{i}");
				AddLabel(label);
			}
		}

		/// <summary>
		/// Generates a dummy label to measure how high it wants to be.
		/// </summary>
		private double CalculateLabelHeight()
		{
			var sizeTest = CreateLabel("size-test");
			sizeTest.Measure(new Size(double.MaxValue, double.MaxValue));
			var labelHeight = sizeTest.DesiredSize.Height;
			return labelHeight;
		}

		protected override void OnInitialized(EventArgs e)
		{
			TimerHelper.Current.TakeSnapshot("mainwnd-oninitialized-start");
			base.OnInitialized(e);
			TimerHelper.Current.TakeSnapshot("mainwnd-oninitialized-base-end");
		}

		/// <summary>
		/// Handles text input in the textbox.
		/// </summary>
		protected abstract void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e);

		/// <summary>
		/// Resets the labels to the given option strings, and scrolls back to the top.
		/// </summary>
		protected void ResetLabels(IEnumerable<string> options)
		{
			scrollOffset = 0;
			optionStrings = options.ToList();
			SetLabelContents(optionStrings);
		}

		/// <summary>
		/// Sets the contents of the labels to the given values,
		/// hiding any labels that did not receive a value.
		/// </summary>
		private void SetLabelContents(List<string> values)
		{
			// First unset our selection.
			UnselectCurrent();
			for (var i = 0; i < Options.Count; i++)
			{
				if (values.Count > i)
				{
					// Only select a label if we've supplied one or more values,
					// and only if it's the first label.
					if (i == 0)
					{
						SelectFirst();
					}
					Options[i].Visibility = Visibility.Visible;
					Options[i].Text = values[i];
				}
				else
				{
					Options[i].Visibility = Visibility.Hidden;
				}
			}
		}

		/// <summary>
		/// Selects a label; deselecting the previously selected label.
		/// </summary>
		/// <param name="label">The label to be selected. If this value is null, the selected label will not be changed.</param>
		protected void Select(SelectionLabel label)
		{
			if (label == null) return;

			if (Selected != null)
			{
				Selected.Background = styleConfig.Options.BackgroundColour;
				Selected.Foreground = styleConfig.Options.TextColour;
				Selected.LabelBorder.BorderBrush = styleConfig.Options.BorderColour;
				Selected.LabelBorder.BorderThickness = styleConfig.Options.BorderWidth;
			}
			Selected = label;
			Selected.Background = styleConfig.Selection.BackgroundColour;
			Selected.Foreground = styleConfig.Selection.TextColour;
			Selected.LabelBorder.BorderBrush = styleConfig.Selection.BorderColour;
			Selected.LabelBorder.BorderThickness = styleConfig.Selection.BorderWidth;
		}

		private void UnselectCurrent()
		{
			if (Selected == null) return;
			Selected.Background = styleConfig.Options.BackgroundColour;
			Selected.Foreground = styleConfig.Options.TextColour;
			Selected.LabelBorder.BorderBrush = styleConfig.Options.BorderColour;
			Selected.LabelBorder.BorderThickness = styleConfig.Options.BorderWidth;
			Selected = null;
		}
		
		/// <summary>
		/// Returns the text on the currently selected label.
		/// </summary>
		/// <returns></returns>
		public string GetSelection()
		{
			return Selected.Text;
		}

		protected SelectionLabel CreateLabel(string content)
		{
			var label = new SelectionLabel(content,
			                               styleConfig.Options,
			                               styleConfig.FontSize,
			                               new FontFamily(styleConfig.FontFamily));

			label.MouseLeftButtonUp += (sender, args) =>
			{
				if (label == Selected)
				{
					HandleSelect();
				}
				else
				{
					Select(label);
					HandleSelectionChange(label);
				}
				// Return focus to the searchbox so the user can continue typing immediately.
				SearchBox.Focus();
			};
			return label;
		}

		protected void AddLabel(SelectionLabel label)
		{
			Options.Add(label);
			OptionsPanel.Children.Add(label);
		}

		protected override void OnActivated(EventArgs e)
		{
			// If this is the first time the window is activated, we need to do a second call to Activate(),
			// otherwise it won't actually gain focus for some reason ¯\_(ツ)_/¯
			if (firstActivation)
			{
				firstActivation = false;
				Activate();
			}
			base.OnActivated(e);

			// Whenever the window is activated, the search box should gain focus.
			if (!isClosing) SearchBox.Focus();
		}

		// Whenever the window loses focus, we reactivate it so it's brought to the front again, allowing it
		// to regain focus.
		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnLostKeyboardFocus(e);
			if (!isClosing && tryRemainOnTop) Activate();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			isClosing = true;
		}

		/// <summary>
		/// Finds the first non-hidden label to the left of the label at the specified index.
		/// </summary>
		/// <param name="index">The position in the label list where searching should begin.</param>
		/// <returns>The first label matching this condition, or null if no matching labels were found.</returns>
		private SelectionLabel FindPrevious(int index)
		{
			var previous = index - 1;
			if (previous >= 0)
			{
				var opt = Options[previous];
				return opt.Visibility == Visibility.Visible ? Options[previous] : FindPrevious(previous);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Finds the first non-hidden label to the right of the label at the specified index.
		/// </summary>
		/// <param name="index">The position in the label list where searching should begin.</param>
		/// <returns>The first label matching this condition, or null if no matching labels were found.</returns>
		private SelectionLabel FindNext(int index)
		{
			var next = index + 1;
			if (next < Options.Count)
			{
				var opt = Options[next];
				return opt.Visibility == Visibility.Visible ? Options[next] : FindNext(next);
			}
			else
			{
				return null;
			}
		}

		protected void SetSearchBoxText(string text)
		{
			SearchBox.Text = text;
			SearchBox.CaretIndex = text.Length;
		}

		protected virtual void HandleSelectionChange(SelectionLabel selection) { }


		protected abstract void HandleSelect();

		protected bool IsPressed(HotkeyConfig hotkey)
		{
			var combination = KeyCombination.Parse(hotkey.Hotkey);

			if (combination.Key != Key.None)
			{
				if (!Keyboard.IsKeyDown(combination.Key)) return false;
			}
			return (Keyboard.Modifiers == combination.ModifierKeys);
		}

		private void SelectNext()
		{
			var selectionIndex = Options.IndexOf(Selected);
			if (selectionIndex < Options.Count)
			{
				// Number of options that we're out of the scrolling bounds
				var boundsOffset = selectionIndex + scrollBoundary + 2 - Options.Count;

				if (boundsOffset <= 0 || scrollOffset + Options.Count >= optionStrings.Count)
				{
					var label = FindNext(selectionIndex);
					Select(label);
					HandleSelectionChange(label);
				}
				else
				{
					scrollOffset += 1;
					var current = Selected;
					SetLabelContents(optionStrings.Skip(scrollOffset).ToList());
					Select(current);
				}

			}
		}

		private void SelectPrevious()
		{
			var selectionIndex = Options.IndexOf(Selected);
			if (selectionIndex >= 0)
			{
				// Number of options that we're out of the scrolling bounds
				var boundsOffset = scrollBoundary + 1 - selectionIndex;

				if (boundsOffset <= 0 || scrollOffset - 1 < 0)
				{
					var label = FindPrevious(selectionIndex);
					Select(label);
					HandleSelectionChange(label);
				}
				else
				{
					scrollOffset -= 1;
					var current = Selected;
					SetLabelContents(optionStrings.Skip(scrollOffset).ToList());
					Select(current);
				}

			}
		}

		private void SelectFirst()
		{
			if (!Options.Any()) return;
			var label = Options.First();
			Select(label);
			HandleSelectionChange(label);
		}

		private void SelectLast()
		{
			if (!Options.Any()) return;
			var label = Options.Last();
			Select(label);
			HandleSelectionChange(label);
		}

		private void SearchBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			var matches = ConfigManager.Config.Interface.Hotkeys.Where(IsPressed).ToList();

			// Prefer manually defined shortcuts over default shortcuts.
			if (matches.Any())
			{
				e.Handled = true;
				foreach (var match in matches)
				{
					switch (match.Action)
					{
						case HotkeyAction.SelectNext:
							SelectNext();
							break;
						case HotkeyAction.SelectPrevious:
							SelectPrevious();
							break;
						case HotkeyAction.SelectFirst:
							SelectFirst();
							break;
						case HotkeyAction.SelectLast:
							SelectLast();
							break;
					}
				}
				return;
			}

			switch (e.Key)
			{
				case Key.Left:
				case Key.Up:
					e.Handled = true;
					SelectPrevious();
					break;
				case Key.Right:
				case Key.Down:
					e.Handled = true;
					SelectNext();
					break;
				case Key.Enter:
					e.Handled = true;
					HandleSelect();
					break;
				case Key.Escape:
					e.Handled = true;
					Close();
					break;
			}
		}

		private void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
			{
				SelectPrevious();
			}
			else if (e.Delta < 0)
			{
				SelectNext();
			}
		}
	}
}
