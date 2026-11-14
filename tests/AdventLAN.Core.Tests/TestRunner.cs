using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AdventLAN.Core.Tests
{
    /// <summary>Markiert eine öffentliche, statische, parameterlose Methode als Test.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestAttribute : Attribute { }

    public sealed class AssertException : Exception
    {
        public AssertException(string message) : base(message) { }
    }

    public static class Assert
    {
        public static void True(bool condition, string message = "Bedingung ist falsch")
        {
            if (!condition) throw new AssertException(message);
        }

        public static void Equal<T>(T expected, T actual)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new AssertException($"Erwartet: {expected}, tatsächlich: {actual}");
        }
    }

    /// <summary>
    /// Mini-Runner ohne NuGet-Pakete. Gibt bei Erfolg eine Zeile aus,
    /// bei Fehlern nur die Fehlerstellen. Exit-Code 0 = grün, 1 = rot.
    /// </summary>
    public static class Program
    {
        public static int Main()
        {
            var tests = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(m => m.GetCustomAttribute<TestAttribute>() != null && m.GetParameters().Length == 0)
                .OrderBy(m => m.DeclaringType!.Name).ThenBy(m => m.Name)
                .ToList();

            var failures = new List<string>();
            foreach (var test in tests)
            {
                try
                {
                    test.Invoke(null, null);
                }
                catch (TargetInvocationException ex)
                {
                    failures.Add($"FEHLER {test.DeclaringType!.Name}.{test.Name}: {ex.InnerException?.Message}");
                }
            }

            if (failures.Count == 0)
            {
                Console.WriteLine($"Tests: {tests.Count} bestanden, 0 fehlgeschlagen");
                return 0;
            }

            foreach (var line in failures) Console.WriteLine(line);
            Console.WriteLine($"Tests: {tests.Count - failures.Count} bestanden, {failures.Count} fehlgeschlagen");
            return 1;
        }
    }
}
