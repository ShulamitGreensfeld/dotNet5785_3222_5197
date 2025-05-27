using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BlApi;
using BO;
using static BO.Enums;

namespace PL
{
    public partial class FilteredCallManagementWindow : Window
    {
        private static readonly IBl s_bl = Factory.Get();

        public FilteredCallManagementWindow(CallStatus filterStatus)
        {
            //InitializeComponent();
            LoadCallList(filterStatus);
        }

        public static readonly DependencyProperty CallListProperty =
            DependencyProperty.Register(nameof(CallList), typeof(IEnumerable<CallInList>), typeof(FilteredCallManagementWindow), new PropertyMetadata(null));

        public IEnumerable<CallInList> CallList
        {
            get => (IEnumerable<CallInList>)GetValue(CallListProperty);
            set => SetValue(CallListProperty, value);
        }

        public static readonly DependencyProperty SelectedCallTypeProperty =
            DependencyProperty.Register(nameof(SelectedCallType), typeof(CallType?), typeof(FilteredCallManagementWindow),
                new PropertyMetadata(null, OnSelectedCallTypeChanged));

        public CallType? SelectedCallType
        {
            get => (CallType?)GetValue(SelectedCallTypeProperty);
            set => SetValue(SelectedCallTypeProperty, value);
        }

        private void LoadCallList(CallStatus filterStatus)
        {
            try
            {
                CallList = s_bl.Call.GetCallsList(CallInListFields.CallStatus, filterStatus);
                if (!CallList?.Any() ?? true)
                {
                    MessageBox.Show("No calls found for the selected status.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void OnSelectedCallTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FilteredCallManagementWindow window && e.NewValue is CallType selectedType)
            {
                try
                {
                    window.CallList = s_bl.Call.GetCallsList(CallInListFields.CallType, selectedType);
                    if (!window.CallList?.Any() ?? true)
                    {
                        MessageBox.Show("No calls found for the selected type.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}