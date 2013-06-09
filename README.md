#PropertyChanged Event Propagation

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
			}
		}
	}
	private string _lastName;
	
	public string FullName
	{
		get { return FirstName + " " + LastName; }
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
	
	
The `DependsOn` attributes forces a `RaisePropertyChanged` to the decorated property, when the dependant properties are changes. In the example above, the property `FullName` will have a `RaisePropertyChanged` every time the `FirstName` or `LastName`are changed.
We can also apply the `DependsOn` to parameterless methods. Instead of Raising the property changed event, the methods are simply invoqued.

	[DependsOn("FullName")]
	public void IncrementFullNameChangedCounter()
	{
		FullNameChangedCounter++;
	}
		
		
This behaviour is intended for ObservableCollection also. For this first version, we only 'look' at changes of the collection itself (add, remove, reset).

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

	
## Possible improvements to this approach

	1. **The event hook should be done using WeakReferences**. 
	Slodge allready stated this in https://github.com/slodge/MvvmCross/issues/300  This was something that I thought about and will implement soon.

	2. **Add the ability to 'listen' to changes of the items inside an ObservableCollection**
	The property names of the raised events, should have the following name: `<ListName>.<PropertyName>`