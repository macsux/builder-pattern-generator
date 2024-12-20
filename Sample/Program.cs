﻿using System;
using BuilderGenerator;
using Newtonsoft.Json;
using MyNamespace;
var dog = new Dog
{
    Name = "Drake",
    Breed = "Husky"
};

Console.WriteLine(JsonConvert.SerializeObject(dog, Formatting.Indented));
var sameDog = dog.Builder
    .WithName("Drake")
    .Build(); // object not cloned since value is the same

Console.WriteLine($"dog is sameDog: {ReferenceEquals(dog,sameDog)}");
        
var differentDog = dog.Builder
    .WithName("WallE")
    .Build();

Console.WriteLine(JsonConvert.SerializeObject(differentDog, Formatting.Indented));
Console.WriteLine($"dog is differentDog: {ReferenceEquals(dog,differentDog)}");

var builder = new Dog.DogBuilder();
var builtUp = builder
    .WithName("New dog")
    .Build();
Console.WriteLine($"Built up:");
Console.WriteLine(JsonConvert.SerializeObject(builtUp, Formatting.Indented));
try
{
    new Dog.DogBuilder()
        .WithBreed("Husky")
        .Build(); // name not set - throws
}
catch (InvalidOperationException ex)
{
    Console.WriteLine(ex.Message);
}

namespace MyNamespace
{
    [GenerateBuilder]
    public partial class Dog
    {
        public required string Name {get; init;}
        public string Breed {get; init;}
    }
}


// class Program
// {
//     static void Main(string[] args)
//     {
//         
//     }
// }