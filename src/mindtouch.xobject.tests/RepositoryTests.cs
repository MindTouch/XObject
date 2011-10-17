using System;
using System.Collections.Generic;
using MindTouch.Xml;
using NUnit.Framework;

namespace MindTouch.Dream.XObject.Tests {

    [TestFixture]
    public class RepositoryTests {

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetBuilder_on_non_IXObject_type_throws() {
            XObjectBuilderRepository repository = new XObjectBuilderRepository();
            repository.GetBuilder(typeof(string));
        }

        public interface ITestXObject : IXObject {
            string Foo { get; }
        }

        [Test]
        public void Repository_returns_same_builder_instance_on_multiple_calls() {
            XObjectBuilderRepository repository = new XObjectBuilderRepository();
            XObjectBuilder<ITestXObject> b1 = repository.GetBuilder<ITestXObject>();
            XObjectBuilder<ITestXObject> b2 = repository.GetBuilder<ITestXObject>();
            Assert.AreSame(b1, b2);
        }

        [Test]
        public void Generic_and_non_generic_GetBuilder_return_same_instance() {
            XObjectBuilderRepository repository = new XObjectBuilderRepository();
            XObjectBuilder<ITestXObject> b1 = repository.GetBuilder<ITestXObject>();
            object b2 = repository.GetBuilder(typeof(ITestXObject));
            Assert.AreSame(b1, b2);
        }

        public class TestDocBuilder : IXPathDocBuilder {
            public XDoc BuildXDoc(XDoc rootDoc, string xpath, int nodeCount) {
                throw new System.NotImplementedException();
            }

            public bool CanHandle(string xpath, bool isNodeCollection) {
                throw new System.NotImplementedException();
            }
        }

        [Test]
        public void Registering_docBuilder_adds_to_internal_collection() {
            TestDocBuilder t = new TestDocBuilder();
            XObjectBuilderRepository repository = new XObjectBuilderRepository();
            repository.RegisterDocBuilder(t);
            List<IXPathDocBuilder> builders = new List<IXPathDocBuilder>(repository.DocBuilders);
            Assert.AreEqual(1, builders.Count);
            Assert.AreSame(t, builders[0]);
        }

        public class TestConverter : IXObjectTypeConverter {
            public bool CanConvert(Type t) {
                return typeof(string).IsAssignableFrom(t);
            }

            public object Convert(XDoc doc) {
                throw new System.NotImplementedException();
            }

            public bool CanConvertBack {
                get { throw new System.NotImplementedException(); }
            }

            public void ConvertBack(XDoc doc, object value) {
                throw new System.NotImplementedException();
            }
        }

        [Test]
        public void Registering_converter_adds_to_internal_collection() {
            TestConverter t = new TestConverter();
            XObjectBuilderRepository repository = new XObjectBuilderRepository();
            repository.RegisterConverter(t);
            List<IXObjectTypeConverter> converters = new List<IXObjectTypeConverter>(repository.TypeConverters);
            Assert.AreEqual(1, converters.Count);
            Assert.AreSame(t, converters[0]);
        }
    }
}
