# Using the Source Generator

This document provides a brief overview of how to use the source generator to generate mapper classes for your entity classes.

## Step 1: Define Your Entity Classes

First, you need to define your entity classes and decorate them with the `CrmEntity` and `CrmField` attributes. These attributes specify the mapping between your entity classes and the corresponding CRM entities.

For example, here is a sample `Person` class decorated with the `CrmEntity` and `CrmField` attributes:

```csharp
[CrmEntity("person")]
public class Person
{
    [CrmField("personid", Mapping = MappingType.PrimaryId)]
    public Guid PersonId { get; set; }

    [CrmField("firstname")]
    public string? FirstName { get; set; }

    [CrmField("lastname")]
    public string? LastName { get; set; }
}
```

## Step 2: Run the Source Generator

The source generator is executed automatically when you build your project. This will generate the mapper classes for each of your entity classes. The mapper classes implement the `IEntityMapper<TEntity>` interface and are responsible for mapping between your entity classes and the corresponding CRM entities.

## Step 3: Use the Mapper Classes

Once the mapper classes have been generated, you can use them to map between your entity classes and the CRM entities. To do this, you need to create an instance of the appropriate mapper class and call its `Map` methods.

For example, here is how you might use the `PersonMapper` class to map between a `Person` object and a CRM entity:

```csharp
var person = new Person { PersonId = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };
var mapper = new PersonMapper();
var entity = mapper.Map(person);
```

You can also use the `Map` method to map from a CRM entity to a `Person` object. Here is an example:

```csharp
var entity = new Entity("person");
entity.Id = Guid.NewGuid();
entity["firstname"] = "Jane";
entity["lastname"] = "Doe";

var mapper = new PersonMapper();
var person = mapper.Map(entity);
```

And that's it! You can now use the source generator to generate mapper classes for your entity classes and easily map between your entity classes and the corresponding CRM entities.
