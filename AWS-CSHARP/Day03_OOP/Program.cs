using System;
using System.Collections.Generic;

namespace Day03_OOP_Basics
{
    // =========================
    // 1. Interface
    // =========================
    public interface IAuditable
    {
        void Audit();
    }

    // =========================
    // 2. Abstract Class
    // =========================
    public abstract class Department
    {
        public string DepartmentName { get; set; }
        public abstract void DepartmentWork();
        public virtual void CommonMeeting()
        {
            Console.WriteLine($"[Department Meeting] Department: {DepartmentName}");
        }
    }

    // =========================
    // 3. Base Class (Employee)
    // =========================
    public class Employee : Department, IAuditable
    {
        // Access Modifiers
        public int Id;                          // Accessible everywhere
        private decimal _salary;                // Accessible only inside this class
        protected string Role;                  // Accessible inside this and derived classes
        internal string InternalNote;           // Accessible within the same assembly
        protected internal string ProtInternal; // Accessible within assembly OR derived classes
        private protected string PrivProt;      // Accessible within containing class OR derived classes in same assembly

        public Employee(int id, decimal salary, string role)
        {
            Id = id;
            _salary = salary;
            Role = role;
            InternalNote = "Internal Note: Only within assembly";
            ProtInternal = "ProtInternal: Within assembly or derived";
            PrivProt = "PrivProt: Within class or derived in same assembly";
        }

        public virtual void Work()
        {
            Console.WriteLine($"Employee #{Id} is working as {Role}");
        }

        // Encapsulation - Control salary access
        public decimal GetSalary() => _salary;
        public void SetSalary(decimal salary)
        {
            if (salary > 0) _salary = salary;
            else Console.WriteLine("Invalid salary amount!");
        }

        // Method Overloading (Compile-time Polymorphism)
        public decimal CalculateSalary() => _salary;
        public decimal CalculateSalary(decimal bonus) => _salary + bonus;

        // Implementing Interface
        public void Audit()
        {
            Console.WriteLine($"Auditing Employee #{Id}");
        }

        // Abstract method from Department
        public override void DepartmentWork()
        {
            Console.WriteLine($"{Role} in {DepartmentName} department is performing tasks.");
        }
    }

    // =========================
    // 4. Derived Class (Manager)
    // =========================
    public class Manager : Employee
    {
        public Manager(int id, decimal salary, string role)
            : base(id, salary, role)
        {
        }

        // Method Overriding (Run-time Polymorphism)
        public override void Work()
        {
            Console.WriteLine($"Manager #{Id} is managing the team.");
        }
    }

    // =========================
    // 5. Sealed Class
    // =========================
    public sealed class PermanentEmployee : Employee
    {
        public PermanentEmployee(int id, decimal salary, string role)
            : base(id, salary, role)
        {
        }

        public override void Work()
        {
            Console.WriteLine($"Permanent Employee #{Id} is working full-time.");
        }
    }

    // =========================
    // 6. Product Class (Encapsulation Example)
    // =========================
    public class Product : IAuditable
    {
        private string _name;
        private decimal _price;

        public string Name
        {
            get => _name;
            set
            {
                if (!string.IsNullOrWhiteSpace(value)) _name = value;
                else Console.WriteLine("Invalid product name!");
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                if (value > 0) _price = value;
                else Console.WriteLine("Invalid product price!");
            }
        }

        public void Audit()
        {
            Console.WriteLine($"Auditing Product: {Name}");
        }
    }

    // =========================
    // 7. Static Class
    // =========================
    public static class Helper
    {
        public static void PrintLine()
        {
            Console.WriteLine(new string('-', 50));
        }
    }

    // =========================
    // 8. Generic Repository
    // =========================
    public class Repository<T>
    {
        private List<T> _items = new List<T>();

        public void Add(T item) => _items.Add(item);

        public void DisplayAll()
        {
            foreach (var item in _items)
                Console.WriteLine(item);
        }
    }

    // =========================
    // 9. Main Program
    // =========================
    internal class Program
    {
        static void Main()
        {
            Helper.PrintLine();
            Console.WriteLine("C# OOP BASICS DEMO - Day 03");
            Helper.PrintLine();

            // CLASS & OBJECT
            var emp = new Employee(1, 50000, "Developer");
            emp.DepartmentName = "IT";
            emp.Work();
            emp.DepartmentWork();
            Console.WriteLine($"Salary: {emp.GetSalary()}");
            Console.WriteLine($"Salary with bonus: {emp.CalculateSalary(5000)}");
            emp.Audit();

            Helper.PrintLine();

            // INHERITANCE & METHOD OVERRIDING
            var mgr = new Manager(2, 80000, "Project Manager");
            mgr.DepartmentName = "Operations";
            mgr.Work();
            mgr.DepartmentWork();

            Helper.PrintLine();

            // SEALED CLASS
            var permEmp = new PermanentEmployee(3, 70000, "Tester");
            permEmp.Work();

            Helper.PrintLine();

            // ENCAPSULATION with Product
            var prod = new Product { Name = "Laptop", Price = 55000 };
            prod.Audit();

            Helper.PrintLine();

            // GENERICS
            var empRepo = new Repository<Employee>();
            empRepo.Add(emp);
            empRepo.Add(mgr);
            empRepo.DisplayAll();

            Helper.PrintLine();

            Console.WriteLine("Abstract vs Interface:");
            Console.WriteLine("- Abstract class can have both implemented and abstract methods, can store state.");
            Console.WriteLine("- Interface only contains method/property signatures, no state.");
            Console.WriteLine("- A class can inherit one abstract class but multiple interfaces.");
        }
    }
}
