using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PassWinmenu.Configuration;
using PassWinmenu.PasswordGeneration;

namespace PassWinmenu.Windows
{
	internal partial class PasswordWindow : IDisposable
	{
		private readonly PasswordGenerator passwordGenerator;

		public PasswordWindow(string filename, PasswordGenerationConfig options)
		{
			WindowStartupLocation = WindowStartupLocation.CenterScreen;
			InitializeComponent();

			passwordGenerator = new PasswordGenerator(options);
			CreateCheckboxes();

			Title = "Add new password";

			AddDefaultMetadata(filename);
			RegeneratePassword();
			Password.Focus();
		}

		private void CreateCheckboxes()
		{
			int colCount = 3;
			int index = 0;
			foreach (var charGroup in passwordGenerator.Options.CharacterGroups)
			{
				int x = index % colCount;
				int y = index / colCount;

				var cbx = new CheckBox
				{
					Name = charGroup.Name,
					Content = charGroup.Name,
					Margin = new Thickness(x * 100, y * 20, 0, 0),
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top,
					IsChecked = charGroup.Enabled,
				};
				cbx.Unchecked += HandleCheckedChanged;
				cbx.Checked += HandleCheckedChanged;
				CharacterGroups.Children.Add(cbx);

				index++;
			}
		}

		private void AddDefaultMetadata(string filename)
		{
			var now = DateTime.Now;
			var extraContent = ConfigManager.Config.PasswordStore.PasswordGeneration.DefaultContent
				.Replace("$filename", filename)
				.Replace("$date", now.ToString("yyyy-MM-dd"))
				.Replace("$time", now.ToString("HH:mm:ss"));
			ExtraContent.Text = extraContent;
		}

		private void RegeneratePassword()
		{
			Password.Text = passwordGenerator.GeneratePassword(Int16.Parse(Length.Text));
			Password.CaretIndex = Password.Text.Length;
		}

		private void Btn_Generate_Click(object sender, RoutedEventArgs e)
		{
			Password.Text = passwordGenerator.GeneratePassword(Int16.Parse(Length.Text));
		}

		private void Btn_OK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}

		private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				DialogResult = false;
				Close();
			}
		}

		private void HandleCheckedChanged(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;
			passwordGenerator.Options.CharacterGroups.First(c => c.Name == checkbox.Name).Enabled = checkbox.IsChecked ?? false;

			RegeneratePassword();
		}

		public void Dispose()
		{
			passwordGenerator.Dispose();
		}

		private void HandleLengthTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !e.Text.All(char.IsDigit);
		}

		private void HandleLengthPasting(object sender, DataObjectPastingEventArgs e)
		{
			if(e.DataObject.GetDataPresent(typeof(String)))
			{
				String text = (String)e.DataObject.GetData(typeof(String));
				if(!text.All(char.IsDigit))
				{
					e.CancelCommand();
				}
			}
			else
			{
				e.CancelCommand();
			}
		}
	}
}
