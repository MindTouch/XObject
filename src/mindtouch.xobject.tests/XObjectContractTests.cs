using System;
using MindTouch.Xml;
using NUnit.Framework;

namespace MindTouch.Dream.XObject.Tests {

    [TestFixture]
    public class XObjectContractTests {
        public interface IBadGetter : IXObject {
            string NoGetter { set; }
        }

        [Test]
        public void Missing_getter_throws_when_XObjectBuilder_is_created() {
            try {
                IXObjectBuilderRepository r = new XObjectBuilderRepository();
                r.GetBuilder<IBadGetter>();
            } catch(XObjectContractException e) {
                Assert.AreEqual("Accessor 'NoGetter' must have a getter", e.Message);
                return;
            }
            Assert.Fail("didn't get the expected contract exception");
        }

        public class CustomTypeWithoutSetter { }
        public class CustomTypeGetter : IXObjectTypeConverter {
            public bool CanConvert(Type t) {
                return typeof(CustomTypeWithoutSetter).IsAssignableFrom(t);
            }
            public object Convert(XDoc doc) { return null; }
            public bool CanConvertBack { get { return false; } }
            public void ConvertBack(XDoc doc, object value) { }
        }

        public interface IBadSetter : IXObject {
            [XObjectTypeConverter(typeof(CustomTypeGetter))]
            CustomTypeWithoutSetter CustomTypeWithoutSetter { get; set; }
        }

        [Test]
        public void Lack_of_setter_throws_when_XObjectBuilder_is_created() {
            try {
                IXObjectBuilderRepository r = new XObjectBuilderRepository();
                r.GetBuilder<IBadSetter>();
            } catch(XObjectContractException e) {
                Assert.AreEqual("Accessor 'CustomTypeWithoutSetter' must have a setter type converter", e.Message);
                return;
            }
            Assert.Fail("didn't get the expected contract exception");
        }

        public class TestXPathDocBuilder : IXPathDocBuilder {
            public static int CustomBuilderCalled;
            public XDoc BuildXDoc(XDoc rootDoc, string xpath, int nodeCount) {
                rootDoc.Elem(xpath);
                CustomBuilderCalled++;
                return rootDoc[xpath];
            }

            public bool CanHandle(string xpath, bool isNodeCollection) {
                return xpath == "abc";
            }
        }

        public interface IFailingXPathDocBuilderTest : IXObject {
            [XObjectPath("xyz")]
            [XObjectXPathDocBuilder(typeof(TestXPathDocBuilder))]
            string Bad { get; set; }
        }

        public interface IPassingXPathDocBuilderTest : IXObject {
            [XObjectPath("abc")]
            [XObjectXPathDocBuilder(typeof(TestXPathDocBuilder))]
            string Good { get; set; }
        }

        [Test]
        public void Should_throw_because_builder_cannot_satisfy_xpath_constraint() {
            try {
                XObjectBuilder.New<IFailingXPathDocBuilderTest>();
            } catch(XObjectContractException e) {
                Assert.AreEqual("The custom builder provided for Accessor 'Bad' cannot handle its xpath", e.Message);
                return;
            }
            Assert.Fail("shouldn't have gotten through without exception");
        }

        [Test]
        public void Should_use_custom_builder() {
            TestXPathDocBuilder.CustomBuilderCalled = 0;
            IPassingXPathDocBuilderTest x = XObjectBuilder.New<IPassingXPathDocBuilderTest>();
            x.Good = "excellent";
            Assert.AreEqual("excellent", x.Good);
            Assert.AreEqual("<passing-x-path-doc-builder-test><abc>excellent</abc></passing-x-path-doc-builder-test>", x.AsDocument.ToString());
            Assert.AreEqual(1, TestXPathDocBuilder.CustomBuilderCalled);
        }
    }
}
