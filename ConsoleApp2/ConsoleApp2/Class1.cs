using System;

public class MyClass
{
    private string name;

    public string Name { get; set; }

    public MyClass() { name = "NoName"; }
    public MyClass(string name) { this.name = name; }

    public void PublicMethod()
    {
        Console.WriteLine("Ім'я: " + name);
        PrivateMethod();
    }

    private void PrivateMethod()
    {
        Console.WriteLine("Приватний метод");
    }
}