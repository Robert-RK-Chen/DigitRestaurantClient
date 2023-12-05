using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Animation;
using Windows.System;
using System.Threading.Tasks;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace DigitRestaurantClient.Resources.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Task.Factory.StartNew(() => AsyncUserProfile());

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            Window.Current.SetTitleBar(AppTitleBar);
            coreTitleBar.LayoutMetricsChanged += CoreTitleBarLayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged += CoreTitleBarIsVisibleChanged;
            Window.Current.Activated += CurrentActivated;
        }

        private void CoreTitleBarLayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            AppTitleBar.Height = coreTitleBar.Height;

            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        private void CoreTitleBarIsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
            {
                AppTitleBar.Visibility = Visibility.Visible;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
        }

        private void CurrentActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            SolidColorBrush defaultForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            SolidColorBrush inactiveForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorDisabledBrush"];

            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                AppTitle.Foreground = inactiveForegroundBrush;
            }
            else
            {
                AppTitle.Foreground = defaultForegroundBrush;
            }
        }

        private readonly List<(string Tag, Type Page)> pages = new List<(string Tag, Type Page)>
        {
            ("Home", typeof(HomePage)),
            ("About", typeof(AboutPage)),
        };

        private void NavigationViewControlDisplayModeChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewDisplayModeChangedEventArgs args)
        {
            const int topIndent = 16;
            const int expandedIndent = 48;
            int minimalIndent = 104;

            if (NavigationViewControl.IsBackButtonVisible.Equals(Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed))
            {
                minimalIndent = 48;
            }

            Thickness currMargin = AppTitleBar.Margin;

            if (sender.PaneDisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top)
            {
                AppTitleBar.Margin = new Thickness(topIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            else if (sender.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal)
            {
                AppTitleBar.Margin = new Thickness(minimalIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            else
            {
                AppTitleBar.Margin = new Thickness(expandedIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
        }

        private void NavViewControlLoaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigated += OnNavigated;

            NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0];
            NavViewControlNavigate("Home", new EntranceNavigationTransitionInfo());

            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += CoreDispatcherAcceleratorKeyActivated;
            Window.Current.CoreWindow.PointerPressed += CoreWindowPointerPressed;
            SystemNavigationManager.GetForCurrentView().BackRequested += SystemBackRequested;
        }

        private void NavViewControlItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                NavViewControlNavigate("settings", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
            else if (args.InvokedItemContainer != null)
            {
                var navItemTag = args.InvokedItemContainer.Tag.ToString();
                NavViewControlNavigate(navItemTag, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }

        private void NavViewControlNavigate(string navItemTag, NavigationTransitionInfo transitionInfo)
        {
            Type page = null;
            if (navItemTag == "settings")
            {
                page = typeof(SettingPage);
            }
            else
            {
                var item = pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
                page = item.Page;
            }
            // Get the page type before navigation so you can prevent duplicate entries in the backstack.
            var preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (!(page is null) && !Type.Equals(preNavPageType, page))
            {
                ContentFrame.Navigate(page, null, transitionInfo);
            }
        }

        private void NavViewControlBackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            TryGoBack();
        }

        private void CoreWindowPointerPressed(CoreWindow sender, PointerEventArgs e)
        {
            // Handle mouse back button.
            if (e.CurrentPoint.Properties.IsXButton1Pressed)
            {
                e.Handled = TryGoBack();
            }
        }

        private void CoreDispatcherAcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs e)
        {
            // When Alt+Left are pressed navigate back
            if (e.EventType == CoreAcceleratorKeyEventType.SystemKeyDown
                && e.VirtualKey == VirtualKey.Left
                && e.KeyStatus.IsMenuKeyDown == true
                && !e.Handled)
            {
                e.Handled = TryGoBack();
            }
        }

        private void SystemBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = TryGoBack();
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            NavigationViewControl.IsBackEnabled = ContentFrame.CanGoBack;
            var item = pages.FirstOrDefault(p => p.Page == e.SourcePageType);

            if (ContentFrame.SourcePageType == typeof(SettingPage))
            {
                NavigationViewControl.SelectedItem = (Microsoft.UI.Xaml.Controls.NavigationViewItem)NavigationViewControl.SettingsItem;
            }
            else if (ContentFrame.SourcePageType == typeof(AboutPage))
            {
                NavigationViewControl.SelectedItem = NavigationViewControl.SelectedItem = NavigationViewControl.FooterMenuItems
                    .OfType<Microsoft.UI.Xaml.Controls.NavigationViewItem>()
                    .First(n => n.Tag.Equals(item.Tag));
            }
            else if (ContentFrame.SourcePageType != null)
            {
                NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems
                    .OfType<Microsoft.UI.Xaml.Controls.NavigationViewItem>()
                    .First(n => n.Tag.Equals(item.Tag));
            }
        }

        private bool TryGoBack()
        {
            if (!ContentFrame.CanGoBack)
            {
                return false;
            }
            if (NavigationViewControl.IsPaneOpen &&
                (NavigationViewControl.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Compact ||
                 NavigationViewControl.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal))
            {
                return false;
            }
            ContentFrame.GoBack();
            return true;
        }

        private void AutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only get results when it was a user typing,
            // otherwise assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                //Set the ItemsSource to be your filtered dataset
                //sender.ItemsSource = dataset;
            }
        }

        private void AutoSuggestBoxSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Set sender.Text. You can use args.SelectedItem to build your text string.
        }

        private void AutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
            }
            else
            {
                // Use args.QueryText to determine what to do.
            }
        }

        private async void AsyncUserProfile()
        {
            // 三秒后让同步提示框收起。
            await Task.Delay(3200);
            await syncInfo.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                syncInfo.IsOpen = false;
            });
        }
    }
}
