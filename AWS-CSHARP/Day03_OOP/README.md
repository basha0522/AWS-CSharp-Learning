 Day 03 – C# OOP Basics

This session covers **Object-Oriented Programming (OOP)** concepts in C#, with examples and explanations.  
We’ll go through **classes, objects, inheritance, access modifiers, method overloading/overriding, encapsulation, abstraction, interfaces, static/sealed classes, and generics**.

---

## 1. Classes and Objects

**Class**: A blueprint or template for creating objects.  
**Object**: An instance of a class containing data (fields) and behavior (methods).

**Example:**
```csharp
public class Car
{
    public string Brand;
    public void Drive()
    {
        Console.WriteLine($"{Brand} is driving.");
    }
}

// Usage
Car myCar = new Car();
myCar.Brand = "Toyota";
myCar.Drive();
2. Inheritance
Inheritance allows one class to acquire the properties and methods of another.

Types:
Single Inheritance – One class inherits from another.

Multilevel Inheritance – A class inherits from a derived class.

Hierarchical Inheritance – Multiple classes inherit from a single base class.

Example:

csharp
Copy
Edit
public class Vehicle
{
    public void Start() => Console.WriteLine("Vehicle started.");
}

public class Car : Vehicle
{
    public void Honk() => Console.WriteLine("Car honks!");
}

public class SportsCar : Car
{
    public void Turbo() => Console.WriteLine("Turbo mode on!");
}
3. Access Modifiers
Access modifiers control visibility of members.

Modifier	Access Scope
public	Accessible anywhere.
private	Accessible only within the same class.
protected	Accessible in the same class and derived classes.
internal	Accessible within the same assembly.
protected internal	Accessible in derived classes OR within the same assembly.
private protected	Accessible in derived classes AND within the same assembly.

Example:

csharp
Copy
Edit
public class BaseClass
{
    private int a = 1;
    protected int b = 2;
    internal int c = 3;
    protected internal int d = 4;
    private protected int e = 5;
}
4. Method Overloading (Compile-time Polymorphism)
Same method name, different parameters.

csharp
Copy
Edit
public class MathOps
{
    public int Add(int a, int b) => a + b;
    public double Add(double a, double b) => a + b;
}
5. Method Overriding (Runtime Polymorphism)
Same method signature in base and derived classes, using virtual and override.

csharp
Copy
Edit
public class Animal
{
    public virtual void Speak() => Console.WriteLine("Animal sound");
}

public class Dog : Animal
{
    public override void Speak() => Console.WriteLine("Bark");
}
6. Encapsulation
Hiding internal details and exposing only required members via properties or methods.

csharp
Copy
Edit
public class BankAccount
{
    private double balance;
    public double Balance
    {
        get => balance;
        set
        {
            if (value >= 0) balance = value;
        }
    }
}
7. Abstraction
Hiding implementation details using abstract classes or interfaces.

csharp
Copy
Edit
public abstract class Shape
{
    public abstract double GetArea();
}

public class Circle : Shape
{
    public double Radius { get; set; }
    public override double GetArea() => Math.PI * Radius * Radius;
}
8. Interfaces
Defines a contract that implementing classes must fulfill.

csharp
Copy
Edit
public interface IWorker
{
    void Work();
}

public class Engineer : IWorker
{
    public void Work() => Console.WriteLine("Engineer working...");
}
9. Static Class
Cannot be instantiated.

Contains only static members.

Used for utility/helper methods.

csharp
Copy
Edit
public static class MathHelper
{
    public static double PI = 3.14;
    public static double Square(double num) => num * num;
}
10. Sealed Class
Cannot be inherited.

Used to prevent further derivation.

csharp
Copy
Edit
public sealed class FinalClass
{
    public void Show() => Console.WriteLine("No inheritance allowed.");
}
11. Generics
Allows defining classes, methods, and interfaces with type parameters.

Example with Employee, Manager, and Product:

csharp
Copy
Edit
public class GenericRepository<T>
{
    private List<T> items = new List<T>();
    public void Add(T item) => items.Add(item);
    public IEnumerable<T> GetAll() => items;
}

public class Employee { public string Name { get; set; } }
public class Manager { public string Name { get; set; } }
public class Product { public string Name { get; set; } }
Abstract Class vs Interface
Feature	Abstract Class	Interface
Implementation Allowed	Yes, can have method implementations	No (C# 8+ allows default methods)
Multiple Inheritance	No	Yes
Fields	Yes	No
Use Case	Base class with shared logic	Contract definition for unrelated classes

When to Use Static and Sealed Classes
Static Class: Use when functionality is not dependent on instance data (e.g., utility methods).

Sealed Class: Use when you want to restrict inheritance for security, performance, or design reasons.

Interview Questions
Explain OOP principles in C# with examples.

Difference between protected internal and private protected.

When would you use a sealed class?

How is abstraction implemented in C#?

Explain method overloading vs overriding.

Can a static class be inherited? Why or why not?

Difference between abstract class and interface.

Explain a real-world use case for generics.

End of Day 03 Learning

yaml
Copy
Edit

---

If you want, I can also **add diagrams** (UML-like flow of Class → Object → Inheritance, etc.) in the README so it looks richer in GitHub. That will make it easier to recall during interviews. Would you like me to add that?








Ask ChatGPT
