This project demonstrates the use of .NET Source generators in order to automatically generate builder pattern for classes. Full details on this project are available in this [blog post](https://stakhov.pro/code-generation-with-net-5-builder-pattern/). Please note that this is a working POC sample rather then a production grade code.

### What does it do

Turns this

```c#
[GenerateBuilder]
public partial class Dog
{
    public required string Name {get; init;}
    public string Breed {get; init;}
}
```

into this (compile time)

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
                Name = _name,Breed = _breed
           };
       }
   }
}

```





