using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReactiveWiki.Views
{
    public partial class WikiView : Page, IViewFor<WikiVM>
    {
        public WikiVM ViewModel { get; set; }
        object IViewFor.ViewModel { get { return ViewModel; } set { ViewModel = (WikiVM)value; } }

        public WikiView()
        {
            InitializeComponent();

            ViewModel = new WikiVM();
            DataContext = ViewModel;

            InitializeAutoCompleteBox();

            this.WhenAnyValue(t => t.ViewModel.URL)
                .Subscribe(url => browser.Navigate(url));

            InitializeSpecial();
        }

        void InitializeAutoCompleteBox()
        {
            // stop AutoCompleteBox from filtering Items
            searchBox.Populating += (s, e) =>
            {
                e.Cancel = true;

                this.WhenAnyObservable(t => t.ViewModel.AutoComplete.ItemsAdded)
                    .Take(1)
                    .Subscribe(x => this.searchBox.PopulateComplete());
            };

            var keyUp = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                h => searchBox.KeyUp += h,
                h => searchBox.KeyUp -= h);

            keyUp
                .Where(ev => ev.EventArgs.Key == Key.Enter)
                .Subscribe(x => ViewModel.Navigate.Execute(null));
        }

        void InitializeSpecial()
        {
            var loaded = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                h => this.Loaded += h,
                h => this.Loaded -= h);

            // Take(1) will automatically unsubscribe from the event after loading.
            loaded.Take(1).Subscribe(x =>
            {
                var window = Window.GetWindow(this);
                var previewKeyUp = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                    h => window.PreviewKeyUp += h,
                    h => window.PreviewKeyUp -= h);

                var keyUpArgs = previewKeyUp
                    .Select(ev => ev.EventArgs);

                var keyPress = keyUpArgs.Select(e => e.Key);

                // Konami Code
                // Create a helper function for creating observables that fire
                // when a specific key is pressed.
                Func<Key, IObservable<Key>> pressedIs =
                    key => keyPress.Where(pressedKey => pressedKey == key);

                // Create a helper function for creating observables that fire
                // when a key other than a specific key is pressed.
                Func<Key, IObservable<Key>> pressedIsNot =
                    key => keyPress.Where(pressedKey => pressedKey != key);

                Func<Key, IObservable<Key>> nextPressedIs =
                    key => pressedIs(key).Take(1).TakeUntil(pressedIsNot(key));

                var konamiCode =
                    from u in pressedIs(Key.Up)
                    from u2 in nextPressedIs(Key.Up)
                    from d in nextPressedIs(Key.Down)
                    from d2 in nextPressedIs(Key.Down)
                    from l in nextPressedIs(Key.Left)
                    from r in nextPressedIs(Key.Right)
                    from l2 in nextPressedIs(Key.Left)
                    from r2 in nextPressedIs(Key.Right)
                    from b in nextPressedIs(Key.B)
                    from a in nextPressedIs(Key.A)
                    from start in nextPressedIs(Key.Enter)
                    select Unit.Default;

                konamiCode.Subscribe(_ => { ViewModel.SearchText = "Konami Code"; ViewModel.Navigate.Execute(null); });
            });
        }
    }
}
