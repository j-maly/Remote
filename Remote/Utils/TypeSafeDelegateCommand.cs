using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace Remote.Utils
{
    public class TypeSafeDelegateCommand<T> : ICommand
    {
        private readonly DelegateCommand<T> underlyingDelegateCommand;

        public TypeSafeDelegateCommand(Action<T> executeMethod) 
        {
            underlyingDelegateCommand = new DelegateCommand<T>(executeMethod);
        }

        public TypeSafeDelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            underlyingDelegateCommand = new DelegateCommand<T>(executeMethod, canExecuteMethod);
        }

        public TypeSafeDelegateCommand(DelegateCommand<T> underlyingDelegateCommand)
        {
            this.underlyingDelegateCommand = underlyingDelegateCommand;
        }

        public bool CanExecute(object parameter)
        {
            return true;
            //if (parameter is T)
            //{
            //    return underlyingDelegateCommand.CanExecute((T) parameter);
            //}
            //return false;
        }

        public void Execute(object parameter)
        {
            if (parameter is T)
            {
                ((ICommand)underlyingDelegateCommand).Execute((T) parameter);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { underlyingDelegateCommand.CanExecuteChanged += value; }
            remove { underlyingDelegateCommand.CanExecuteChanged -= value; }
        }

        public static ICommand FromAsyncHandler(Func<ServiceInfo, Task> execute)
        {
            var c = DelegateCommand<ServiceInfo>.FromAsyncHandler(execute);
            return new TypeSafeDelegateCommand<ServiceInfo>(c);
        }
    }
}