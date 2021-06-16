using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CSharp.RuntimeBinder;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;

namespace AdvancedTopics
{
    public abstract class Persona
    {
        public int YearBirthDay { get; set; }
        public int GetAge()
        {
            return DateTime.Today.Year - YearBirthDay;
        }

        public  virtual void ImprimirAno()
        {
            Console.WriteLine($"El ano es {YearBirthDay}");
        }
        public abstract void ImprimirEdad();
    }

    public class Estudiante : Persona
    {
        public override void ImprimirEdad()
        {
            Console.WriteLine($"El estudiante tiene una edad de {this.GetAge()}");
        }

        public override void ImprimirAno()
        {
            Console.WriteLine($"El Estudiante nació el {this.YearBirthDay}");
        }
    }

    public class Profesor : Persona
    {
        public override void ImprimirEdad()
        {
            Console.WriteLine($"El profesor tiene una edad de {this.GetAge()}");

        }
    }
    public class RepeatAttribute : Attribute
    {
        public int Times { get; }

        public RepeatAttribute(int times)
        {
            Times = times;
        }
    }

    public class Widget : DynamicObject
    {
        public void WhatIsThis()
        {
            Console.WriteLine(This.World);
        }

        public dynamic This => this;
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = binder.Name;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length == 1)
            {
                result = new string('*',(int)indexes[0]);
                return true;
            }

            result = null;
            return false;
        }
    }

    public class DynamicXMLObject : DynamicObject
    {
        private XElement node;

        public DynamicXMLObject(XElement _node)
        {
            this.node = _node;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var elemen = node.Element(binder.Name);
            if (elemen != null)
            {
                result = new DynamicXMLObject(elemen);
                return true;
            }
            else
            {
                var attribute = node.Attribute(binder.Name);

                if (attribute != null)
                {
                    result = attribute.Value;
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
        }
    }


    public class Foo
    {
        public string Name => "Foo";
    }

    public class FooDerived : Foo
    {
        public string Name => "FooDerived";
    }

    public class Person1
    {
        public string Name;
        public int Age;

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, " +
                   $"{nameof(Age)}: {Age}";
        }
    }

    public static class ExtensionMethods
    {//    ↑↑↑↑↑↑ must be static

        // extension on your own type
        public static int Measure(this Foo foo)
        { //   ↑↑↑↑↑↑  
            return foo.Name.Length;
        }

        // extension method on an existing type (incl. primitive type)
        public static string ToBinary(this int n)
        {
            return Convert.ToString(n, 2);
        }

        // extension on an interface
        public static void Save(this ISerializable serializable)
        {
            // 
        }

        // you don't get extension method polymorphism
        public static int Measure(this FooDerived derived)
        {
            return 42;
        }

        // it doesn't work as an override
        public static string ToString(this Foo foo)
        {
            return "test";
        }

        // extension methods on value tuples
        public static Person1 ToPerson(this (string name, int age) data)
        {
            return new Person1 { Name = data.name, Age = data.age };
        }

        // extension on a generic type
        public static int Measure<T, U>(this Tuple<T, U> t)
        {
            return t.Item2.ToString().Length;
        }

        // extension on a delegate
        public static Stopwatch Measure(this Func<int> action)
        {
            var st = new Stopwatch();
            st.Start();
            action();
            st.Stop();
            return st;
        }


        private static Dictionary<WeakReference, object> data
            = new Dictionary<WeakReference, object>();

        public static object GetTag(this object o)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            return key != null ? data[key] : null;
        }

        public static void SetTag(this object o, object tag)
        { 
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null)
            {
                data[key] = tag;
            }
            else
            {
                data.Add(new WeakReference(o), tag);
            }
        }



        // name shortening wrapper
        public static StringBuilder al(this StringBuilder sb, string text)
        {
            return sb.AppendLine(text);
        }

        // combined extension method
        // does two or more things in one
        public static StringBuilder AppendFormatLine(
            this StringBuilder sb, string format, params object[] args)
        {
            return sb.AppendFormat(format, args).AppendLine();
        }

        // composite extension method: pairwise operation on elements
        public static ulong Xor(params ulong[] values)
        {
            ulong first = values[0];
            foreach (var x in values.Skip(1))
                first ^= x;
            return first;
        }

        // params extension method
        // improves API usability
        public static void AddRange<T>(this IList<T> list, params T[] objects)
        {
            foreach (T obj in objects)
                list.Add(obj);
        }

        // antistatic extension method
        // moving a static member into an extension
        public static string f(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        // factory extension methods
        public static DateTime June(this int day, int year)
        {
            return new DateTime(year, 6, day);
        }

    }

    public static class Maybe
    {
        public static TResult With<TInput, TResult>(this TInput o,
            Func<TInput, TResult> evaluator)
            where TResult : class
            where TInput : class
        {
            if (o == null) return null;
            else return evaluator(o);
        }

        public static TInput If<TInput>(this TInput o,
            Func<TInput, bool> evaluator)
            where TInput : class
        {
            if (o == null) return null;
            return evaluator(o) ? o : null;
        }

        public static TInput Do<TInput>(this TInput o, Action<TInput> action)
            where TInput : class
        {
            if (o == null) return null;
            action(o);
            return o;
        }

        public static TResult Return<TInput, TResult>(this TInput o,
            Func<TInput, TResult> evaluator, TResult failureValue)
            where TInput : class
        {
            if (o == null) return failureValue;
            return evaluator(o);
        }

        public static TResult WithValue<TInput, TResult>(this TInput o,
            Func<TInput, TResult> evaluator)
            where TInput : struct
        {
            return evaluator(o);
        }

    }

    public class Person
    {
        public Address Address { get; set; }
    }

    public class Address
    {
        public string PostCode { get; set; }
    }

    public class Program
    {
        public void MyMethod(Person p)
        {
            //      string postcode;
            //      if (p != null)
            //      {
            //        if (HasMedicalRecord(p) && p.Address != null)
            //        {
            //          CheckAddress(p.Address);
            //          if (p.Address.PostCode != null)
            //            postcode = p.Address.PostCode.ToString();
            //          else
            //            postcode = "UNKNOWN";
            //        }
            //      }
            string postcode = p.With(x => x.Address).With(x => x.PostCode);

            postcode = p
                .If(HasMedicalRecord)
                .With(x => x.Address)
                .Do(CheckAddress)
                .Return(x => x.PostCode, "UNKNOWN");
        }

        private void CheckAddress(Address pAddress)
        {
            throw new NotImplementedException();
        }

        private bool HasMedicalRecord(Person person)
        {
            throw new NotImplementedException();
        }


        public event EventHandler<int> MyEvent;

        public void Handler(object sender, int arg)
        {
            Console.WriteLine($"I just got {arg} from {sender?.GetType().Name}");
        }

        [RepeatAttribute(3)]
        public void SomeMethod()
        {

        }

        static void Main(string[] args)
        {
            #region Types 

            #endregion

            #region Invocation

            var s = "abracadabra   ";
            var t = typeof(string);

            Console.WriteLine(t);

            var trimMethod = t.GetMethod("Trim", Array.Empty<Type>());
            Console.WriteLine(trimMethod);

            var result = trimMethod.Invoke(s, Array.Empty<object>());
            Console.WriteLine(result);

            var numberString = "123";
            var parseMethod = typeof(int).GetMethod("TryParse",
                   new[]{ typeof(string), typeof(int).MakeByRefType() });
            Console.WriteLine(parseMethod);
            object[] args1 = { numberString, null };
            var ok = parseMethod.Invoke(null, args1);
            Console.WriteLine(ok);
            Console.WriteLine(args1[1]);

            var at = typeof(Activator);
            var method = at.GetMethod("CreateInstance", Array.Empty<Type>());
            Console.WriteLine(method);

            var ciGeneric = method.MakeGenericMethod(typeof(Guid));
            Console.WriteLine(ciGeneric);

            var guid = ciGeneric.Invoke(null, null);
            Console.WriteLine(guid);
            #endregion

            #region Delegate and Events
            var demo = new Program();

            var eventInfo = typeof(Program).GetEvent("MyEvent");
            var handlerMethod = demo.GetType().GetMethod("Handler");

            // we need a delegate of a particular type
            var handler = Delegate.CreateDelegate(
                eventInfo.EventHandlerType,
                null, // object that is the first argument of the method the delegate represents
                handlerMethod
            );
            eventInfo.AddEventHandler(demo, handler);

            demo.MyEvent?.Invoke(null, 312);


            #endregion

            #region Attributes
            var sm = typeof(Program).GetMethod("SomeMethod");
            foreach (var att in sm.GetCustomAttributes(true))
            {
                Console.WriteLine("Found an attribute: " + att.GetType());
                if (att is RepeatAttribute ra)
                {
                    Console.WriteLine($"Need to repeat {ra.Times} times");
                }
            }


            #endregion

            #region Dynamic

            dynamic d = "Hello";
            Console.WriteLine(d.GetType());
            Console.WriteLine(d.Length);

            d += " World!!!";
            Console.WriteLine(d);

            #endregion

            #region Dynamic Object
            var widget = new Widget() as dynamic;

            Console.WriteLine(widget.Hello);
            Console.WriteLine(widget[5]);

            widget.WhatIsThis();

            #endregion

            #region Dynamic for XML

            var xml = @"
<people>
    <person name='Jorge Luis'/>
</people>
";
            var node = XElement.Parse(xml);
            var name = node.Element("person").Attribute("name");
            Console.WriteLine(name?.Value);

            dynamic dyn = new DynamicXMLObject(node);
            Console.WriteLine(dyn.person.name);

            #endregion

            #region Expando Object

            dynamic person = new ExpandoObject();
            person.FirstName = "Jorge Luis";
            person.Age = 40;


            Console.WriteLine($"{person.FirstName} is {person.Age} years old!!");

            person.Address = new ExpandoObject();
            person.Address.Country  = "MX";
            person.Address.City = "Guadalajara";
            person.Address.PostalCode = "45020";

            person.SayHello = new Action(() =>
            {
                Console.WriteLine("Hello!!!!");
            });

            person.SayHello();

            person.FallsIII = null;
            person.FallsIII += new EventHandler<dynamic>((sender, o) =>
            {
                Console.WriteLine($"Wee need a doctor for {o}");
            });

            EventHandler<dynamic> e = person.FallsIII;
            e?.Invoke(person,person.FirstName);


            var dict = (IDictionary<string, object>) person;
            Console.WriteLine(dict.ContainsKey("FirstName"));
            Console.WriteLine(dict.ContainsKey("LastName"));

            #endregion

            #region Extension Methods
            var foo = new Foo();
            Console.WriteLine(foo.Measure());

            

            Console.WriteLine(foo);
            Console.WriteLine(ExtensionMethods.ToString(foo));

            var derived = new FooDerived();
            Foo parent = derived;
            Console.WriteLine("As parent: " + parent.Measure());
            Console.WriteLine("As child:  " + derived.Measure());

            Console.WriteLine(42.ToBinary());

            Person1 p = ("Jorge Luis", 22).ToPerson();
            Console.WriteLine(p);

            Console.WriteLine(Tuple.Create(12, "hello").Measure());

            Func<int> calculate = delegate
            {
                Thread.Sleep(1000);
                return 42;
            };
            var st = calculate.Measure();
            Console.WriteLine($"took {st.ElapsedMilliseconds} msec");


            #endregion

            #region Extension Methods Persistence
            string s1 = "Meaning of life";
            s1.SetTag(422);
            Console.WriteLine(s1.GetTag());
            #endregion

            #region Extension Methods Patterns
            var items = new List<int>();
            items.AddRange(1, 2, 3);

            var name1 = "John";
            var greeting = "My name is {0}".f(name1);

            var notToday = 10.June(2020);

            #endregion


            #region MyTest Herencia

            var persona = new Estudiante() { YearBirthDay = 1981 };
            persona.ImprimirAno();
            ((Persona)persona).ImprimirAno();
            var profesor = new Profesor() { YearBirthDay = 1978 };
            profesor.ImprimirAno();

            #endregion

        }
         
    }
}
