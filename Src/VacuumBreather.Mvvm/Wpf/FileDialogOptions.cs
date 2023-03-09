namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Represents data which is passed to a file dialog.</summary>
public readonly record struct FileDialogOptions(string InitialDirectory, string Filter, bool RestoreDirectory);