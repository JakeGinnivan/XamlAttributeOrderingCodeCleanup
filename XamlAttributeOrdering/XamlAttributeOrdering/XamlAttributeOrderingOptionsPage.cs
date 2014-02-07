using System;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ReSharper.Features.Common.Options.Languages;
using JetBrains.UI.Application;
using JetBrains.UI.Controls;
using JetBrains.UI.CrossFramework;
using JetBrains.UI.Options;
using JetBrains.UI.Options.Helpers;

namespace XamlAttributeOrdering
{
    [OptionsPage(Pid, "Xaml Attribute Ordering", null, ParentId = XamlPage.PID, Sequence = 100)]
    public class XamlAttributeOrderingOptionsPage : AOptionsPage
    {
        private const string Pid = "XamlAttributeOrdering";

        public XamlAttributeOrderingOptionsPage([NotNull] Lifetime lifetime, OptionsSettingsSmartContext settings, UIApplication environment)
            : base(lifetime, environment, Pid)
        {
            if (lifetime == null) throw new ArgumentNullException("lifetime");

            Control = InitView(lifetime, settings);
        }

        private EitherControl InitView(Lifetime lifetime, OptionsSettingsSmartContext settings)
        {
            var grid = new Grid { Background = SystemColors.ControlBrush };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 0
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 1
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 2
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 3
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 4
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 5
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 6
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 7

            var enabledBox = new CheckBoxDisabledNoCheck2 { Content = "Enable ordering of Xaml Attributes" };
            Grid.SetColumnSpan(enabledBox, 2);
            Grid.SetRow(enabledBox, 0);
            settings.SetBinding<XamlAttributeOrderingSettings, bool>(lifetime, x => x.Enable, enabledBox, CheckBoxDisabledNoCheck2.IsCheckedLogicallyDependencyProperty);
            grid.Children.Add(enabledBox);

            var alphabeticOrderBox = new CheckBoxDisabledNoCheck2 { Content = "Order alphabetically instead of using groups" };
            Grid.SetColumnSpan(alphabeticOrderBox, 2);
            Grid.SetRow(alphabeticOrderBox, 1);
            settings.SetBinding<XamlAttributeOrderingSettings, bool>(lifetime, x => x.IsAlphabeticalOrder, alphabeticOrderBox, CheckBoxDisabledNoCheck2.IsCheckedLogicallyDependencyProperty);
            grid.Children.Add(alphabeticOrderBox);

            var group1Label = new Label { Content = "Group #1 - Keys" };
            Grid.SetRow(group1Label, 2);
            var group1Box = new TextBox();
            Grid.SetRow(group1Box, 2);
            Grid.SetColumn(group1Box, 1);
            settings.SetBinding<XamlAttributeOrderingSettings, string>(lifetime, x => x.Group1_KeyGroup, group1Box, TextBox.TextProperty);
            grid.Children.Add(group1Label);
            grid.Children.Add(group1Box);

            var group2Label = new Label { Content = "Group #2 - Names" };
            Grid.SetRow(group2Label, 3);
            var group2Box = new TextBox();
            Grid.SetRow(group2Box, 3);
            Grid.SetColumn(group2Box, 1);
            settings.SetBinding<XamlAttributeOrderingSettings, string>(lifetime, x => x.Group2_NameGroup, group2Box, TextBox.TextProperty);
            grid.Children.Add(group2Label);
            grid.Children.Add(group2Box);

            var group3Label = new Label { Content = "Group #3 - Attached Layout Properties" };
            Grid.SetRow(group3Label, 4);
            var group3Box = new TextBox();
            Grid.SetRow(group3Box, 4);
            Grid.SetColumn(group3Box, 1);
            settings.SetBinding<XamlAttributeOrderingSettings, string>(lifetime, x => x.Group3_AttachedLayoutGroup, group3Box, TextBox.TextProperty);
            grid.Children.Add(group3Label);
            grid.Children.Add(group3Box);

            var group4Label = new Label { Content = "Group #4 - Layout Properties" };
            Grid.SetRow(group4Label, 5);
            var group4Box = new TextBox();
            Grid.SetRow(group4Box, 5);
            Grid.SetColumn(group4Box, 1);
            settings.SetBinding<XamlAttributeOrderingSettings, string>(lifetime, x => x.Group4_LayoutGroup, group4Box, TextBox.TextProperty);
            grid.Children.Add(group4Label);
            grid.Children.Add(group4Box);

            var group5Label = new Label { Content = "Group #5 - Alignment Properties" };
            Grid.SetRow(group5Label, 6);
            var group5Box = new TextBox();
            Grid.SetRow(group5Box, 6);
            Grid.SetColumn(group5Box, 1);
            settings.SetBinding<XamlAttributeOrderingSettings, string>(lifetime, x => x.Group5_AlignmentGroup, group5Box, TextBox.TextProperty);
            grid.Children.Add(group5Label);
            grid.Children.Add(group5Box);

            var group6Label = new Label { Content = "Group #6 - Misc Properties" };
            Grid.SetRow(group6Label, 7);
            var group6Box = new TextBox();
            Grid.SetRow(group6Box, 7);
            Grid.SetColumn(group6Box, 1);
            settings.SetBinding<XamlAttributeOrderingSettings, string>(lifetime, x => x.Group6_MiscGroup, group6Box, TextBox.TextProperty);
            grid.Children.Add(group6Label);
            grid.Children.Add(group6Box);

            return grid;
        }
    }
}