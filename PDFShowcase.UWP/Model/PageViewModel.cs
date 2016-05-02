using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PDFShowcase.UWP.Model
{
    public class PageViewModel
    {        
        private DelegateCommand<bool> _showCommand;

        public PageModel Model { get; set; }
        public DelegateCommand<bool> ShowCommand
        {
            get
            {
                return _showCommand ??
                    (_showCommand = new DelegateCommand<bool>(
                    x =>
                    {

                        Timer timer = new Timer(new TimerCallback(a =>
                          {
                              this.Model.IsShow = false;
                          }), this.Model.IsShow, 2000, 2000);
                    }));
            }
        }
    }

    #region DelegateCommand


    public class DelegateCommand<T> : ICommand
    {
        private Action<T> _Command;
        private Func<T, bool> _CanExecute;
        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> command) : this(command, null) { }

        public DelegateCommand(Action<T> command, Func<T, bool> canexecute)
        {
            if (command == null)
            {
                throw new ArgumentException("command");
            }
            _Command = command;
            _CanExecute = canexecute;
        }

        public bool CanExecute(object parameter)
        {
            return _CanExecute == null ? true : _CanExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _Command((T)parameter);
        }

    }
    #endregion
}