using Cirrious.MvvmCross.ViewModels;
using PropertyChangedEventPropagation.Core.Attributes;
using PropertyChangedEventPropagation.Core.Extensions;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;

namespace PropertyChangedEventPropagation.Core.ViewModels
{
    public class FirstViewModel : ViewModel
    {
        #region Properties

        public string FirstName
        {
            get { return _fristName; }
            set
            {
                if (_fristName != value)
                {
                    _fristName = value;
                    RaisePropertyChanged(() => FirstName);
                }
            }
        }
        private string _fristName;

        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    RaisePropertyChanged(() => LastName);
                }
            }
        }
        private string _lastName;
        
        [DependsOn("FirstName")]
        [DependsOn("LastName")]
        public string FullName
        {
            get { return "{0} {1}".FormatTemplate(FirstName, LastName); }
        }

        [DependsOn("FullName")]
        public bool IsFullNameValid
        {
            get { return !FirstName.IsNullOrEmpty() && !LastName.IsNullOrEmpty(); }
        }

        public int FullNameChangedCounter
        {
            get { return _fullNameChangedCounter; }
            set
            {
                if (_fullNameChangedCounter != value)
                {
                    _fullNameChangedCounter = value;
                    RaisePropertyChanged(() => FullNameChangedCounter);
                }
            }
        }
        private int _fullNameChangedCounter;

        public ObservableCollection<string> NamesList
        {
            get { return _namesList; }
        }
        private readonly ObservableCollection<string> _namesList = new ObservableCollection<string>();

        [DependsOn("NamesList")]
        public int NamesListCounter
        {
            get { return NamesList.Count; }
        }

        public string SelectedName
        {
            get { return _selectedName; }
            set
            {
                if (_selectedName != value)
                {
                    _selectedName = value;
                    RaisePropertyChanged(() => SelectedName);
                }
            }
        }
        private string _selectedName;

        [DependsOn("SelectedName")]
        public bool IsNameSelected
        {
            get { return SelectedName != null; }
        }

        #endregion

        #region Commands

        public ICommand AddCommand
        {
            get { return _addCommand; }
        }
        private readonly ICommand _addCommand;

        public ICommand RemoveCommand
        {
            get { return _removeCommand; }
        }
        private readonly ICommand _removeCommand;

        #endregion

        #region Constructor

        public FirstViewModel()
        {
            _addCommand = new MvxCommand(OnAdd);
            _removeCommand = new MvxCommand(OnRemove);
        }

        #endregion

        #region Methods

        [DependsOn("FullName")]
        public void IncrementFullNameChangedCounter()
        {
            FullNameChangedCounter++;
        }

        private void OnAdd()
        {
            if (!FirstName.IsNullOrEmpty() &&
                !LastName.IsNullOrEmpty() &&
                !NamesList.Contains(FullName))
            {
                NamesList.Add(FullName);
                
                FirstName = null;
                LastName = null;
            }
        }

        private void OnRemove()
        {
            if (SelectedName != null)
            {
                NamesList.Remove(SelectedName);
                SelectedName = null;
            }
        }

        #endregion
    }
}
