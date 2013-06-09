#PropertyChanged Event Propagation
=================================

This is my approach for enabling property changed event propagation across the ViewModels in an Mvvm approach.
The goal is to be able to use attributes decoration to map dependencies between properties and/or methods within the ViewModel

This is a follow up of a request made in the MvvmCross repo: https://github.com/slodge/MvvmCross/issues/300


The main goal is to transform something like this:

	public string FirstName
	{
		get { return _fristName; }
		set
		{
			if (_fristName != value)
			{
				_fristName = value;
				RaisePropertyChanged(() => FirstName);
				RaisePropertyChanged(() => FullName);
				IncrementFullNameChangedCounter();
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
				RaisePropertyChanged(() => FullName);
				IncrementFullNameChangedCounter();
			}
		}
	}
	private string _lastName;
	
	public string FullName
	{
		get { return FirstName + " " + LastName; }
	}
	
	public void IncrementFullNameChangedCounter()
	{
		//some logic....
	}
	
Into something like

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
		get { return FirstName + " " + LastName; }
	}
	
	[DependsOn("FullName")]
	public void IncrementFullNameChangedCounter()
	{
		//some logic....
	}
	