This project demonstrates the use of .NET Source generators in order to automatically generate builder pattern for classes. Full details on this project are available in this [blog post](https://stakhov.pro/code-generation-with-net-5-builder-pattern/). 

The code has been updated in 2024 since that blog was written to support new syntax constructs in C#. This builder implementation creates an immutable pattern that minimizes memory heap allocations and minimizes excessive cloning.

### What does it do

For code like this:

```c#
[GenerateBuilder]
public partial class Dog
{
    public required string Name {get; init;}
    public string Breed {get; init;}
}
```

It allows you to do this:
```c#
// start from initializer syntax
var dog = new Dog
{
    Name = "Drake",
    Breed = "Husky"
};

// start from builder
dog = new Dog.DogBuilder()
    .WithName("Drake")
    .WithBreed("Husky")
    .Build();
    
    
var anotherDog = dog.Builder
    .WithName("WallE")
    .Build(); // clone dog with new name

var builder = new Dog.DogBuilder();
builder.WithBreed("Husky").Build(); // throws because required property Name is not set
```

By generating this:

```c#

partial class Dog
{
    public DogBuilder Builder => new DogBuilder(this);
    public struct DogBuilder
    {
        private byte _set;
        Dog _original;

        public DogBuilder(Dog original)
        {
            _original = original;
        }
        
        private string _name;
        public DogBuilder WithName(string name)
        {
            _name = name;
            _set |= 1;
            return this;
        }
        private bool IsNameSet => (_set & 1) == 1;
        
        private string _breed;
        public DogBuilder WithBreed(string breed)
        {
            _breed = breed;
            _set |= 2;
            return this;
        }
        private bool IsBreedSet => (_set & 2) == 2;
        
        

        public Dog Build()
        {
            if(_original == null)
            {
                 if(!IsNameSet)
                 {
                     var message = $"The following required properties have not been set: {(!IsNameSet ? "Name" : "")}, ";
                     throw new InvalidOperationException(message.TrimEnd(',',' '));
                 }

                return new Dog
                {
                    Name = _name,Breed = _breed
                };
            }

            if(IsNameSet && !object.Equals(_name, _original.Name))
            {
                goto clone;
            }
            if(IsBreedSet && !object.Equals(_breed, _original.Breed))
            {
                goto clone;
            }
            
           return _original;
           clone:
           return new Dog
           {
            Name = IsNameSet ? _name : _original.Name,
            Breed = IsBreedSet ? _breed : _original.Breed
           };
       }
   }
}

```





