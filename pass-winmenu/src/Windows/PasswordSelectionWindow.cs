using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace PassWinmenu.Windows
{
	internal class PasswordSelectionWindow : SelectionWindow
	{
		private readonly List<string> options;

		public PasswordSelectionWindow(IEnumerable<string> options, SelectionWindowConfiguration configuration) : base(configuration)
		{
			this.options = options.ToList();
			ResetLabels(this.options);
		}

		protected override void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			// We split on spaces to allow the user to quickly search for a certain term, as it allows them
			// to search, for example, for reddit.com/username by entering "re us"
			var terms = SearchBox.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			var matching = options.Where((option) =>
			{
				var lcOption = option.ToLower(CultureInfo.CurrentCulture);
				return terms.All(term =>
				{
					// Perform case-sensitive matching if the user entered an uppercase character.
					if (term.Any(char.IsUpper))
					{
						if (option.Contains(term)) return true;
					}
					else
					{
						if (lcOption.Contains(term)) return true;
					}
					return false;
				});
			});
			ResetLabels(matching);
		}

		protected override void HandleSelect()
		{
			if (Selected != null)
			{
				Success = true;
				Close();
			}
		}
	}
}
