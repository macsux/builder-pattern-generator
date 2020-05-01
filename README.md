This project demonstrates the use of .NET 5 Source generators in order to automatically generate builder pattern for classes. Full details on this project are available in this [blog post](https://stakhov.pro/code-generation-with-net-5-builder-pattern/). Please note that this is a working POC sample rather then a production grade code.

### What does it do

Turns this

```c#
[GenerateBuilder]
public partial class Person
{
    [Required]
    public string FirstName { get; private set; }
    [Required]
    public string LastName { get; private set; }
    public DateTime? BirthDate { get; private set; }
}
```

into this (compile time)

```c#
partial class Person
{
  private Person(){}
  public static PersonBuilder Builder => new PersonBuilder();
  public class PersonBuilder
  {

    private string _firstName;
    public PersonBuilder FirstName(string FirstName)
    {
      _firstName = FirstName;
      return this;
    }


    private string _lastName;
    public PersonBuilder LastName(string LastName)
    {
      _lastName = LastName;
      return this;
    }


    private DateTime? _birthDate;
    public PersonBuilder BirthDate(DateTime? BirthDate)
    {
      _birthDate = BirthDate;
      return this;
    }


    public Person Build()
    {
      Validate();
      return new Person
      {
        FirstName = _firstName,
        LastName = _lastName,
        BirthDate = _birthDate,

      };
    }
    public void Validate()
    {
      void AddError(Dictionary<string, string> items, string property, string message)
      {
        if (items.TryGetValue(property, out var errors))
          items[property] = $"{errors}\n{message}";
        else
          items[property] = message;
      }
      Dictionary<string,string> errors = new Dictionary<string, string>();
      if(_firstName == default)  AddError(errors, "FirstName", "Value is required");
      if(_lastName == default)  AddError(errors, "LastName", "Value is required");

      if(errors.Count > 0)
        throw new BuilderCommon.BuilderException(errors);
    }
  }
}

```



### Requirements

- Visual Studio 2019 preview
- .NET 5.0 preview

