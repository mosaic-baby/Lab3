using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Переписанный класс команд, который ГАРАНТИРОВАННО держит кнопки активными
public class RelayCommand : ICommand
{
    private readonly Action _execute;

    public RelayCommand(Action execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public bool CanExecute(object parameter)
    {
        return true; // Кнопка ВСЕГДА активна для нажатия
    }

    public void Execute(object parameter)
    {
        _execute();
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}
