using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BuilderGenerator
{
    [Generator]
    public class BuilderSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            
        }

        public void Execute(GeneratorExecutionContext context)
        {
            
            // using the context, get a list of syntax trees in the users compilation
            foreach (var syntaxTree in context.Compilation.SyntaxTrees)
            {
                var classBuilders = GenerateBuilder(syntaxTree, context.Compilation);
                // add the filepath of each tree to the class we're building
                foreach (var classBuilder in classBuilders)
                {
                    context.AddSource($"{classBuilder.Key}.Builder.g.cs", SourceText.From(classBuilder.Value, Encoding.UTF8));
                }
                
            }
         

            // inject the created source into the users compilation
            
        }

        public static  Dictionary<string, string> GenerateBuilder(SyntaxTree syntaxTree, Compilation compilation)
        {
            
            var classToBuilder = new Dictionary<string, string>();
            var cu = (CompilationUnitSyntax)syntaxTree.GetRoot();
            var classesWithAttribute = cu
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(cds => cds.AttributeLists.HasAttribute(nameof(GenerateBuilderAttribute)))
                .ToList();

            foreach (var classDeclaration in classesWithAttribute)
            {
                string className = null!;
                try
                {
                    className = classDeclaration.Identifier.Text;
                    var diag = new StringBuilder();

                    var namespaceName = compilation.GetSemanticModel(syntaxTree).GetDeclaredSymbol(classDeclaration)?
                                            .ContainingNamespace.Name;
                    var namespaceDeclaration = !string.IsNullOrEmpty(namespaceName) ? $"namespace {namespaceName};" : "";
                    
                    var properties = classDeclaration.DescendantNodes()
                        .OfType<PropertyDeclarationSyntax>()
                        .Where(x => x.Modifiers.All(m => m.ToString() != "static"))
                        .Select(x => new BuilderPropertyInfo(x))
                        .ToList();
                    var builderName = $"{className}Builder";


                    var requiredProperties = properties.Where(x => x.IsRequired).ToList();
                    //language=csharp
                    var builderTemplate =
                        $$"""

                          {{@namespaceDeclaration}}
                          {{cu.Usings}}
                          partial class {{@className}}
                          {
                              public {{@builderName}} Builder => new {{@builderName}}(this);
                              public struct {{@builderName}}
                              {
                                  private byte _set;
                                  {{@className}} _original;
                      
                                  public {{@builderName}}({{@className}} original)
                                  {
                                      _original = original;
                                  }
                                  
                                  {{properties.Render((p, i) => $$"""
                                                                  private string {{p.BackingFieldName}};
                                                                  public {{@builderName}} With{{p.Name}}({{p.Type}} {{p.ParameterName}})
                                                                  {
                                                                      {{p.BackingFieldName}} = {{p.ParameterName}};
                                                                      _set |= {{i + 1}};
                                                                      return this;
                                                                  }
                                                                  private bool Is{{p.Name}}Set => (_set & {{i + 1}}) == {{i + 1}};


                                                                  """).Ident(2)}}
                      
                                  public {{@className}} Build()
                                  {
                                      if(_original == null)
                                      {
                                           if({{requiredProperties.Render(r => $"!Is{r.Name}Set", " || ")}})
                                           {
                                               var message = $"The following required properties have not been set: {{requiredProperties.Render(r => $$"""{(!Is{{r.Name}}Set ? "{{r.Name}}" : "")}, """)}}";
                                               throw new InvalidOperationException(message.TrimEnd(',',' '));
                                           }
                      
                                          return new {{@className}}
                                          {
                                              {{properties.Render(p => $$"""
                                                                         {{p.Name}} = {{p.BackingFieldName}}
                                                                         """, ",").Ident(4)}}
                                          };
                                      }
                      
                                      {{properties.Render(p => /*language=csharp*/ $$"""
                                            if(Is{{p.Name}}Set && !object.Equals({{p.BackingFieldName}}, _original.{{p.Name}}))
                                            {
                                                goto clone;
                                            }

                                            """).Ident(3)}}
                                     return _original;
                                     clone:
                                     return new {{@className}}
                                     {
                                          {{properties.Render(p => /*language=csharp*/$$"""
                                                {{p.Name}} = {{p.BackingFieldName}}
                                                """, ",").Ident(3)}}
                                     };
                                 }
                             }
                          }
                          """;
                    classToBuilder[className] = builderTemplate;
                }
                catch (Exception e)
                {
                    classToBuilder[className] = $"/*\n{e.ToString()}\n/*";
                }
            }


            return classToBuilder;
        }


        struct BuilderPropertyInfo
        {
            public BuilderPropertyInfo(PropertyDeclarationSyntax property) : this()
            {
                IsRequired = property.Modifiers.Any(x => x.ToString() == "required");
                Type = property.Type.ToString();
                Name = property.Identifier.ToString();
                ParameterName = $"{Name[0].ToString().ToLower()}{Name.Remove(0, 1)}";
                BackingFieldName = $"_{ParameterName}";
            }

            public bool IsRequired { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string ParameterName { get; set; }
            public string BackingFieldName { get; set; }
        }



    }
}

internal static class Extensions
{
    public static string Ident(this object source, int identLevels)
    {
        var lines = source.ToString().TrimStart(' ').Split('\n');
        var ident = new string(' ', identLevels * 4);
        return string.Join("\n", lines.Select((x, i) => $"""{ (i > 0 ? ident : "") }{x}"""));
    }

    public static string Render<T>(this IEnumerable<T> source, Func<T, string> template, string separator = "")
    {
        return string.Join(separator, source.Select(template));
    }

    public static string Render<T>(this IEnumerable<T> source, Func<T, int, string> template, string separator = "")
    {
        return string.Join(separator, source.Select(template));
    }
}