using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Bindables;
using JetBrains.Annotations;
using MangaScraper.Core.Helpers;

namespace MangaScraper.UI.Core.Main {
    /// <summary>
    /// Interaction logic for MultiSelectComboBox.xaml
    /// </summary>
    public partial class MultiSelectComboBox : UserControl {
        private readonly ObservableCollection<Node> _nodeList = new ObservableCollection<Node>();

        public MultiSelectComboBox() {
            InitializeComponent();
        }

        #region Dependency Properties

        [DependencyProperty(OnPropertyChanged = nameof(OnItemsSourceChanged))]
        public Dictionary<string, object> ItemsSource { get; set; }

        [DependencyProperty(OnPropertyChanged = nameof(OnSelectedItemsChanged))]
        public ObservableConcurrentDictionary<string, object> SelectedItems { get; set; }

        [DependencyProperty()]
        public string Text { get; set; }

        [DependencyProperty()]
        public string DefaultText { get; set; }

        #endregion

        #region Events

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs _) {
            var control = (MultiSelectComboBox) d;
            control.DisplayInControl();
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs _) {
            var control = (MultiSelectComboBox) d;
            control.SelectNodes();
            control.SetText();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e) {
            var clickedBox = (CheckBox) sender;

            if (clickedBox.Content as string == "All") {
                _nodeList.ForEach(node => node.IsSelected = clickedBox.IsChecked ?? false);
            }
            else {
                _nodeList.First(i => i.Title == "All").IsSelected = _nodeList.All(s => s.IsSelected || s.Title == "All");
            }

            SetSelectedItems();
            SetText();
        }

        #endregion

        public event EventHandler SelectionChanged;

        #region Methods

        private void SelectNodes() {
            foreach (var keyValue in SelectedItems) {
                var node = _nodeList.FirstOrDefault(i => i.Title == keyValue.Key);
                if (node != null)
                    node.IsSelected = true;
            }
        }

        private void SetSelectedItems() {
            if (SelectedItems == null)
                SelectedItems = new ObservableConcurrentDictionary<string, object>();
            //else
            //  ((ICollection<KeyValuePair<string, object>>)SelectedItems).Clear();
            foreach (var node in _nodeList.Where(node => node.Title != "All")) {
                if (node.IsSelected)
                    SelectedItems.Add(node.Title, ItemsSource[node.Title]);
                else
                    SelectedItems.Remove(node.Title);
            }

            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void DisplayInControl() {
            _nodeList.Clear();
            if (ItemsSource.Count > 0)
                _nodeList.Add(new Node("All"));
            foreach (var keyValue in ItemsSource) {
                _nodeList.Add(new Node(keyValue.Key));
            }

            MultiSelectCombo.ItemsSource = _nodeList;
        }

        private void SetText() {
            Text = _nodeList.Any(i => i.Title == "All" && i.IsSelected)
                ? "All"
                : string.Join(", ", _nodeList.Where(s => s.IsSelected && s.Title != "All").Select(s => s.Title));

            // set DefaultText if nothing else selected
            if (string.IsNullOrEmpty(Text)) {
                Text = DefaultText;
            }
        }

        #endregion
    }

    public class Node : INotifyPropertyChanged {
        public Node(string title) => Title = title;

        public string Title { get; set; }

        public bool IsSelected { get; set; }

        [UsedImplicitly]
        #pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
    }
}