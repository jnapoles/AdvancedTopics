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

namespace AdvancedTopics
{
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

    public class Program
    {
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
        }
         
    }
}
