/*
 * MindTouch Dream - a distributed REST framework 
 * Copyright (C) 2006-2011 MindTouch, Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit wiki.developer.mindtouch.com;
 * please review the licensing section.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using LinFu.DynamicProxy;
using MindTouch.Dream.XObject.TypeConverter;
using MindTouch.Xml;

namespace MindTouch.Dream.XObject {

    public class XObjectPathAttribute : Attribute {
        public readonly string XPath;
        public XObjectPathAttribute(string XPath) {
            this.XPath = XPath;
        }
    }

    public class XObjectRootAttribute : Attribute {
        public readonly string RootElementName;
        public XObjectRootAttribute(string rootElementName) {
            RootElementName = rootElementName;
        }
    }

    public class XObjectTypeConverterAttribute : Attribute {
        public readonly Type Type;
        public XObjectTypeConverterAttribute(Type type) {
            Type = type;
        }
        public void Validate() {
            if(!typeof(IXObjectTypeConverter).IsAssignableFrom(Type)) {
                throw new XObjectContractException("The converter '{0}' must be a assignable from type IXObjectTypeConverter", Type);
            }
        }
    }

    public class XObjectXPathDocBuilderAttribute : Attribute {
        public readonly Type Type;
        public XObjectXPathDocBuilderAttribute(Type type) {
            Type = type;
        }
        public void Validate() {
            if(!typeof(IXPathDocBuilder).IsAssignableFrom(Type)) {
                throw new XObjectContractException("The builder '{0}' must be a assignable from type IXPathDocBuilder", Type);
            }
        }
    }

    public static class XObjectBuilder {
        private static IXObjectBuilderRepository _repository = new XObjectBuilderRepository();
        public static T New<T>() where T : IXObject {
            return _repository.GetBuilder<T>().New();
        }

        public static T FromXDoc<T>(XDoc doc) where T : IXObject {
            return _repository.GetBuilder<T>().FromXDoc(doc);
        }

        public static T FromXml<T>(string xml) where T : IXObject {
            return _repository.GetBuilder<T>().FromXml(xml);
        }

        public static void SetRepository(IXObjectBuilderRepository repository) {
            _repository = repository;
        }
    }

    public class XObjectBuilder<T> where T : IXObject {
        private static readonly ProxyFactory _proxyFactory = new ProxyFactory();

        private readonly Type _type;
        private readonly Dictionary<string, InterceptionRecord> _memberLookup = new Dictionary<string, InterceptionRecord>();
        private readonly string _rootElement;
        private readonly IXObjectBuilderRepository _repository;


        public XObjectBuilder(IXObjectBuilderRepository repository) {
            _type = typeof(T);
            _repository = repository;
            var propertyLookup = new Dictionary<string, object>();

            // map repository converters
            var converters = new Dictionary<Type, IXObjectTypeConverter>();
            foreach(IXObjectTypeConverter converter in repository.TypeConverters) {
                Type converterType = converter.GetType();
                if(converters.ContainsKey(converterType)) {
                    continue;
                }
                converters.Add(converterType, converter);
            }

            // get converters registered against the class
            foreach(XObjectTypeConverterAttribute attr in _type.GetCustomAttributes(typeof(XObjectTypeConverterAttribute), true)) {
                attr.Validate();
                if(converters.ContainsKey(attr.Type)) {
                    continue;
                }
                try {
                    converters.Add(attr.Type, (IXObjectTypeConverter)Activator.CreateInstance(attr.Type));
                } catch(Exception e) {
                    throw new XObjectContractException("Cannot create instance of converter '{0}'", attr.Type);
                }
            }


            // the default docBuilder is always first
            var docBuilders = new List<IXPathDocBuilder> { new XPathDocBuilder() };

            // then get doc builders registered against class, they are the next in line
            foreach(XObjectXPathDocBuilderAttribute attr in _type.GetCustomAttributes(typeof(XObjectXPathDocBuilderAttribute), true)) {
                attr.Validate();
                docBuilders.Add((IXPathDocBuilder)Activator.CreateInstance(attr.Type));
            }
            docBuilders.AddRange(repository.DocBuilders);

            // get rootElement
            var attrs = _type.GetCustomAttributes(typeof(XObjectRootAttribute), true);
            if(attrs.Length > 1) {
                throw new XObjectContractException("More than one XObjectRootAttribute on interface '{0}'", _type.Name);
            }
            if(attrs.Length == 1) {
                var rootAttribute = attrs[0] as XObjectRootAttribute;
                _rootElement = rootAttribute.RootElementName;
            } else {

                // divine the rootElement from the type name
                string typeName = _type.Name;
                if(typeName.StartsWith("I")) {
                    typeName = typeName.Substring(1);
                }
                if(typeName.Contains("_")) {
                    throw new XObjectContractException("Type name of '{0}' cannot contain an underscore if no XObjectRootAttribute was specified", _type.Name);
                }
                _rootElement = DivineXPath(typeName);
            }

            // build invocation lookups for all properties
            foreach(var propertyInfo in _type.GetProperties()) {
                Type propType = propertyInfo.PropertyType;
                bool isCollection = false;
                bool isMutableCollection = false;

                // check whether it's a collection and if so, what it's actual type is
                if(propType == typeof(byte[])) {
                    // byte arrays are special and not considered collections
                } else if(propType.IsArray) {
                    propType = propType.GetElementType();
                    isCollection = true;
                } else if(propType.IsGenericType) {
                    Type genericType = propType.GetGenericTypeDefinition();
                    if(genericType == typeof(IList<>) || genericType == typeof(ICollection<>) || genericType == typeof(IEnumerable<>)) {
                        if(genericType != typeof(IEnumerable<>)) {
                            isMutableCollection = true;
                        }
                        propType = propType.GetGenericArguments()[0];
                        isCollection = true;
                    }
                }

                // accessors have to have getters for XObject to work
                if(!propertyInfo.CanRead) {
                    throw new XObjectContractException("Accessor '{0}' must have a getter", propertyInfo.Name);
                }

                // get xpath
                attrs = propertyInfo.GetCustomAttributes(typeof(XObjectPathAttribute), true);
                if(attrs != null && attrs.Length > 1) {
                    throw new XObjectContractException("More than one XObjectPathAttribute on Accessor '{0}'", propertyInfo.Name);
                }
                string xpath;
                if(attrs != null && attrs.Length == 1) {
                    var xpathAttribute = attrs[0] as XObjectPathAttribute;
                    xpath = xpathAttribute.XPath;
                } else {
                    xpath = DivineXPath(propertyInfo.Name);
                }

                if(xpath.StartsWith("/")) {
                    throw new XObjectContractException(string.Format("xpath '{0}' is invalid: must use relative paths"), xpath);
                }

                // check for custom xpath builder
                TypeBuildXPathDelegate xpathDocBuilderDelegate = null;
                attrs = propertyInfo.GetCustomAttributes(typeof(XObjectXPathDocBuilderAttribute), true);
                if(attrs.Length > 1) {
                    throw new XObjectContractException("More than one XObjectXPathDocBuilderAttribute on Accessor '{0}'", propertyInfo.Name);
                }
                if(attrs.Length == 1) {
                    var builderAttribute = attrs[0] as XObjectXPathDocBuilderAttribute;
                    builderAttribute.Validate();
                    var builder = Activator.CreateInstance(builderAttribute.Type) as IXPathDocBuilder;
                    if(!builder.CanHandle(xpath, isCollection)) {
                        throw new XObjectContractException("The custom builder provided for Accessor '{0}' cannot handle its xpath", propertyInfo.Name);
                    }
                    xpathDocBuilderDelegate = builder.BuildXDoc;
                }

                // get convert and convert back delegates
                var convertDelegates = new Tuplet<TypeConvertDelegate, TypeConvertBackDelegate>();
                attrs = propertyInfo.GetCustomAttributes(typeof(XObjectTypeConverterAttribute), true);
                if(attrs.Length > 1) {
                    throw new XObjectContractException("More than one XObjectTypeConverterAttribute on Accessor '{0}'", propertyInfo.Name);
                }
                if(attrs.Length == 1) {

                    // use attribute to determine converter
                    var converterAttribute = attrs[0] as XObjectTypeConverterAttribute;
                    converterAttribute.Validate();
                    IXObjectTypeConverter converter;
                    if(!converters.TryGetValue(converterAttribute.Type, out converter)) {
                        try {
                            converter = (Activator.CreateInstance(converterAttribute.Type) as IXObjectTypeConverter);
                            converters.Add(converterAttribute.Type, (IXObjectTypeConverter)Activator.CreateInstance(converterAttribute.Type));
                        } catch(Exception e) {
                            throw new XObjectContractException("Cannot create instance of converter '{0}'", converterAttribute.Type);
                        }
                    }
                    if(!converter.CanConvert(propType)) {
                        throw new XObjectContractException("Specified converter '{0}' on accessor '{1}' cannot convert type '{2}'", converterAttribute.Type, propertyInfo.Name, propType);
                    }
                    convertDelegates.Item1 = converter.Convert;
                    if(converter.CanConvertBack) {
                        convertDelegates.Item2 = converter.ConvertBack;
                    }
                } else {

                    // see if there is an instance we can use based on the accessor type
                    foreach(IXObjectTypeConverter converter in converters.Values) {
                        if(!converter.CanConvert(propType)) {
                            continue;
                        }
                        convertDelegates.Item1 = converter.Convert;
                        if(converter.CanConvertBack) {
                            convertDelegates.Item2 = converter.ConvertBack;
                        }
                        break;
                    }

                    if(convertDelegates.Item1 == null) {
                        // finally fall through to delegate divination
                        convertDelegates = DivineDelegate(propertyInfo, propType);
                    }
                }
                if(propertyInfo.CanWrite || isMutableCollection) {
                    if(convertDelegates.Item2 == null) {
                        throw new XObjectContractException("Accessor '{0}' must have a setter type converter", propertyInfo.Name);
                    }
                    if(xpathDocBuilderDelegate == null) {
                        foreach(IXPathDocBuilder builder in docBuilders) {
                            if(builder.CanHandle(xpath, false)) {
                                xpathDocBuilderDelegate = builder.BuildXDoc;
                            }
                        }
                    }
                    if(xpathDocBuilderDelegate == null) {
                        throw new XObjectContractException("A setter for XPath '{0}' cannot be created by default converter", xpath);
                    }
                }

                // inject collection handling to wrap the collection type
                NodeCountDelegate nodeCountDelegate = null;
                if(isCollection) {
                    Type collectionType = propertyInfo.PropertyType;
                    TypeConvertDelegate convertDelegate = convertDelegates.Item1;
                    TypeConvertBackDelegate convertBackDelegate = convertDelegates.Item2;
                    if(collectionType.IsArray) {
                        convertDelegates.Item1 = delegate(XDoc d) {
                            Array array = Array.CreateInstance(propType, d.ListLength);
                            int i = 0;
                            foreach(XDoc element in d) {
                                array.SetValue(convertDelegate(element), i++);
                            }
                            return array;
                        };
                        convertDelegates.Item2 = delegate(XDoc d, object v) {
                            int i = 0;
                            Array array = v as Array;
                            foreach(XDoc node in d) {
                                convertBackDelegate(node, array.GetValue(i++));
                            }
                        };
                        nodeCountDelegate = delegate(object v) { return ((Array)v).Length; };
                    } else if(collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() != typeof(Nullable<>)) {
                        Type listType = typeof(List<>).MakeGenericType(propType);
                        convertDelegates.Item1 = delegate(XDoc d) {
                            IList list = Activator.CreateInstance(listType) as IList;
                            foreach(XDoc element in d) {
                                list.Add(convertDelegate(element));
                            }
                            return list;
                        };
                        convertDelegates.Item2 = delegate(XDoc d, object v) {
                            int i = 0;
                            IList list = v as IList;
                            foreach(XDoc node in d) {
                                convertBackDelegate(node, list[i++]);
                            }
                        };
                        nodeCountDelegate = delegate(object v) { return ((IList)v).Count; };
                    }
                } else {
                    nodeCountDelegate = delegate { return 1; };
                }

                // create and register record of interception information to be used by XObjectInterceptor
                InterceptionRecord record = new InterceptionRecord(
                    xpath,
                    convertDelegates.Item1,
                    (propertyInfo.CanWrite || isMutableCollection) ? convertDelegates.Item2 : null,
                    xpathDocBuilderDelegate,
                    nodeCountDelegate);
                _memberLookup.Add(propertyInfo.Name, record);

                // this is just remembers an accessor we've seen so that the method checker knows they're valid
                propertyLookup.Add(propertyInfo.GetGetMethod().Name, null);
                if(propertyInfo.CanWrite) {
                    propertyLookup.Add(propertyInfo.GetSetMethod().Name, null);
                }
            }

            // make sure there are no other methods than the property getters and setters already registered above
            foreach(MethodInfo methodInfo in _type.GetMethods()) {
                if(!propertyLookup.ContainsKey(methodInfo.Name)) {
                    throw new XObjectContractException("XObject contract may only contain Properties");
                }
            }
        }

        public T New() {
            return FromXDoc(null);
        }

        public T FromXDoc(XDoc doc) {
            doc = doc ?? new XDoc(_rootElement);
            return _proxyFactory.CreateProxy<T>(new XObjectInterceptor(_type, _memberLookup, doc));
        }

        public T FromXml(string xml) {
            return FromXDoc(XDocFactory.From(xml, MimeType.TEXT_XML));
        }

        private string DivineXPath(string name) {
            bool isAttribute = !Char.IsUpper(name[0]);
            int lastSegmentStart = 0;
            int index = 0;
            StringBuilder sb = new StringBuilder();
            foreach(char c in name) {
                if(c == '_') {
                    sb.Append('/');
                    index++;
                    lastSegmentStart = index;
                    isAttribute = !Char.IsUpper(name[lastSegmentStart]);
                    continue;
                }
                if(index != lastSegmentStart && Char.IsUpper(c)) {
                    sb.Append('-');
                    index++;
                }
                sb.Append(Char.ToLower(c));
                index++;
            }
            if(isAttribute) {
                sb.Insert(lastSegmentStart, '@');
            }
            return sb.ToString();
        }

        private Tuplet<TypeConvertDelegate, TypeConvertBackDelegate> DivineDelegate(PropertyInfo propInfo, Type propType) {
            TypeConvertDelegate convertDelegate;
            TypeConvertBackDelegate convertBackDelegate = delegate(XDoc d, object v) { d.ReplaceValue(v); };
            if(propType == typeof(XDoc)) {
                convertDelegate = delegate(XDoc d) { return d; };
                convertBackDelegate = delegate(XDoc d, object v) {
                    XDoc clone = ((XDoc)v).Clone();
                    d.RemoveNodes();
                    d.AddAll(clone["*"]);
                    foreach(XDoc attr in clone["@*"]) {
                        d.Attr(attr.Name, attr.Contents);
                    }
                };
            } else if(typeof(IXObject).IsAssignableFrom(propType)) {
                object builder = _repository.GetBuilder(propType);
                Type builderType = builder.GetType();
                convertDelegate = delegate(XDoc d) {
                    return builderType.InvokeMember(
                        "FromXDoc",
                        BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public,
                        null,
                        builder,
                        new object[] { d });
                };
                convertBackDelegate = delegate(XDoc d, object v) {
                    // Note (arnec): should the clone really happen here? how to avoid clones when the data hasn't changed.
                    XDoc child = ((IXObject)v).AsDocument.Clone();
                    d.RemoveNodes();
                    d.AddAll(child["*"]);
                    foreach(XDoc attr in child["@*"]) {
                        d.Attr(attr.Name, attr.Contents);
                    }
                };
            } else if(propType == typeof(byte[])) {
                convertDelegate = delegate(XDoc d) { return d.AsBytes; };
                convertBackDelegate = delegate(XDoc d, object v) { d.ReplaceValue(v); };
            } else if(propType.IsEnum ||
             (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>) && propType.GetGenericArguments()[0].IsEnum)
                ) {
                Type genericType = typeof(XObjectEnumConverter<>);
                Type converterType = genericType.MakeGenericType(propType);
                IXObjectTypeConverter converter = (IXObjectTypeConverter)Activator.CreateInstance(converterType);
                convertDelegate = converter.Convert;
                convertBackDelegate = converter.ConvertBack;
            } else if(propType == typeof(bool)) {
                convertDelegate = delegate(XDoc d) { return d.AsBool.GetValueOrDefault(); };
            } else if(propType == typeof(bool?)) {
                convertDelegate = delegate(XDoc d) { return d.AsBool; };
            } else if(propType == typeof(byte)) {
                convertDelegate = delegate(XDoc d) { return d.AsByte.GetValueOrDefault(); };
            } else if(propType == typeof(byte?)) {
                convertDelegate = delegate(XDoc d) { return d.AsByte; };
            } else if(propType == typeof(DateTime)) {
                convertDelegate = delegate(XDoc d) { return d.AsDate.GetValueOrDefault(); };
            } else if(propType == typeof(DateTime?)) {
                convertDelegate = delegate(XDoc d) { return d.AsDate; };
            } else if(propType == typeof(decimal)) {
                convertDelegate = delegate(XDoc d) { return d.AsDecimal.GetValueOrDefault(); };
            } else if(propType == typeof(decimal?)) {
                convertDelegate = delegate(XDoc d) { return d.AsDecimal; };
            } else if(propType == typeof(double)) {
                convertDelegate = delegate(XDoc d) { return d.AsDouble.GetValueOrDefault(); };
            } else if(propType == typeof(double?)) {
                convertDelegate = delegate(XDoc d) { return d.AsDouble; };
            } else if(propType == typeof(float)) {
                convertDelegate = delegate(XDoc d) { return d.AsFloat.GetValueOrDefault(); };
            } else if(propType == typeof(float?)) {
                convertDelegate = delegate(XDoc d) { return d.AsFloat; };
            } else if(propType == typeof(int)) {
                convertDelegate = delegate(XDoc d) { return d.AsInt.GetValueOrDefault(); };
            } else if(propType == typeof(int?)) {
                convertDelegate = delegate(XDoc d) { return d.AsInt; };
            } else if(propType == typeof(long)) {
                convertDelegate = delegate(XDoc d) { return d.AsLong.GetValueOrDefault(); };
            } else if(propType == typeof(long?)) {
                convertDelegate = delegate(XDoc d) { return d.AsLong; };
            } else if(propType == typeof(sbyte)) {
                convertDelegate = delegate(XDoc d) { return d.AsSByte.GetValueOrDefault(); };
            } else if(propType == typeof(sbyte?)) {
                convertDelegate = delegate(XDoc d) { return d.AsSByte; };
            } else if(propType == typeof(short)) {
                convertDelegate = delegate(XDoc d) { return d.AsShort.GetValueOrDefault(); };
            } else if(propType == typeof(short?)) {
                convertDelegate = delegate(XDoc d) { return d.AsShort; };
            } else if(propType == typeof(string)) {
                convertDelegate = delegate(XDoc d) { return d.AsText; };
            } else if(propType == typeof(uint)) {
                convertDelegate = delegate(XDoc d) { return d.AsUInt.GetValueOrDefault(); };
            } else if(propType == typeof(uint?)) {
                convertDelegate = delegate(XDoc d) { return d.AsUInt; };
            } else if(propType == typeof(ulong)) {
                convertDelegate = delegate(XDoc d) { return d.AsULong.GetValueOrDefault(); };
            } else if(propType == typeof(ulong?)) {
                convertDelegate = delegate(XDoc d) { return d.AsULong; };
            } else if(propType == typeof(XUri)) {
                convertDelegate = delegate(XDoc d) { return d.AsUri; };
            } else if(propType == typeof(ushort)) {
                convertDelegate = delegate(XDoc d) { return d.AsUShort.GetValueOrDefault(); };
            } else if(propType == typeof(ushort?)) {
                convertDelegate = delegate(XDoc d) { return d.AsUShort; };
            } else {
                throw new XObjectContractException("Unable to divine type conversion for type '{0}' on Accessor '{1}'", propType, propInfo.Name);
            }
            return new Tuplet<TypeConvertDelegate, TypeConvertBackDelegate>(convertDelegate, convertBackDelegate);
        }
    }

    public class XObjectContractException : Exception {
        public XObjectContractException(string message) : base(message) { }
        public XObjectContractException(string message, params object[] args) : base(string.Format(message, args)) { }
    }


}
