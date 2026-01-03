using System;
using System.Windows;
using System.Windows.Input;
using PsyTrancePlayer.ViewModels;

namespace PsyTrancePlayer;

public partial class MainWindow : Window
{
    private bool _isDragging = false;

    public MainWindow()
    {
        InitializeComponent();

        // Pause visualization during window dragging to prevent lag
        this.LocationChanged += OnLocationChanged;
        this.Deactivated += OnDeactivated;
        this.Activated += OnActivated;
        this.MouseLeftButtonDown += OnMouseLeftButtonDown;
        this.MouseLeftButtonUp += OnMouseLeftButtonUp;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Detect if user is starting to drag the window
        if (e.ChangedButton == MouseButton.Left && e.Source == this)
        {
            _isDragging = true;
            if (DataContext is MainViewModel vm)
                vm.PauseVisualization();
        }
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Resume when drag ends
        if (_isDragging)
        {
            _isDragging = false;
            if (DataContext is MainViewModel vm)
                vm.ResumeVisualization();
        }
    }

    private void OnLocationChanged(object? sender, EventArgs e)
    {
        // Window is being moved - pause visualization
        if (DataContext is MainViewModel vm && !_isDragging)
        {
            vm.PauseVisualization();
            _isDragging = true;
        }
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        // Window lost focus - pause visualization to save resources
        if (DataContext is MainViewModel vm)
            vm.PauseVisualization();
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        // Window gained focus - resume visualization
        if (DataContext is MainViewModel vm)
        {
            vm.ResumeVisualization();
            _isDragging = false;
        }
    }
}
