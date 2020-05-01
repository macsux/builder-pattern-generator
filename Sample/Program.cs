using System;
using System.ComponentModel.DataAnnotations;
using BuilderCommon;
using Newtonsoft.Json;

namespace Sample
{
    [GenerateBuilder]
    public partial class Person
    {
        [Required]
        public string FirstName { get; private set; }
        [Required]
        public string LastName { get; private set; }
        public DateTime? BirthDate { get; private set; }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var myDataClass = Person.Builder
                .LastName("blah")
                .Build();
            
            Console.WriteLine(JsonConvert.SerializeObject(myDataClass, Formatting.Indented));

            Console.WriteLine("==== Builder Validation ====");
            try
            {
                Person.Builder 
                    .FirstName("John")
                    .Build();
            }
            catch (BuilderException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

