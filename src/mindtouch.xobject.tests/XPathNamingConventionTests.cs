using MindTouch.Xml;
using NUnit.Framework;

namespace MindTouch.Dream.XObject.Tests {

    [TestFixture]
    public class XPathNamingConventionTests {
        public interface INamingConventionTest : IXObject {
            string attr { get;}
            string attrWithHyphen { get;}
            string Elem { get;}
            string ElemWithHyphen { get;}
            string Sub_Path { get;}
            string Sub_Path_To_attr { get;}
        }

        [Test]
        public void Naming_convention_should_parse_attribute() {
            INamingConventionTest x = XObjectBuilder.FromXDoc<INamingConventionTest>(new XDoc("doc").Attr("attr", "found"));
            Assert.AreEqual("found", x.attr);
        }

        [Test]
        public void Naming_convention_should_parse_attribute_with_hyphen() {
            INamingConventionTest x = XObjectBuilder.FromXDoc<INamingConventionTest>(new XDoc("doc").Attr("attr-with-hyphen", "found"));
            Assert.AreEqual("found", x.attrWithHyphen);
        }

        [Test]
        public void Naming_convention_should_parse_element() {
            INamingConventionTest x = XObjectBuilder.FromXDoc<INamingConventionTest>(new XDoc("doc").Elem("elem", "found"));
            Assert.AreEqual("found", x.Elem);
        }

        [Test]
        public void Naming_convention_should_parse_element_with_hyphen() {
            INamingConventionTest x = XObjectBuilder.FromXDoc<INamingConventionTest>(new XDoc("doc").Elem("elem-with-hyphen", "found"));
            Assert.AreEqual("found", x.ElemWithHyphen);
        }

        [Test]
        public void Naming_convention_should_parse_xpath() {
            INamingConventionTest x = XObjectBuilder.FromXDoc<INamingConventionTest>(
                new XDoc("doc")
                    .Start("sub")
                        .Elem("path", "found")
                    .End());
            Assert.AreEqual("found", x.Sub_Path);
        }

        [Test]
        public void Naming_convention_should_parse_xpath_to_attribute() {
            INamingConventionTest x = XObjectBuilder.FromXDoc<INamingConventionTest>(
                new XDoc("doc")
                    .Start("sub")
                        .Start("path")
                        .Start("to")
                            .Attr("attr", "found")
                    .EndAll());
            Assert.AreEqual("found", x.Sub_Path_To_attr);
        }

        public interface IWithLeadingI : IXObject {
            string Foo { get; }
        }

        [Test]
        public void New_divines_doc_root_drops_leading_i() {
            IWithLeadingI x = XObjectBuilder.New<IWithLeadingI>();
            Assert.AreEqual("<with-leading-i />", x.AsDocument.ToString());
        }

        public interface NoLeadingITest : IXObject {
            string Foo { get; }
        }

        [Test]
        public void New_divines_doc_root_and_does_not_care_about_leading_i() {
            NoLeadingITest x = XObjectBuilder.New<NoLeadingITest>();
            Assert.AreEqual("<no-leading-i-test />", x.AsDocument.ToString());
        }

        public interface Bad_Name_For_Root_Divinitation : IXObject {
            string Foo { get; }
        }

        [Test]
        public void Bad_class_name_throws_if_no_XObjectRoot_attribute_is_specified() {
            try {
                XObjectBuilder.New<Bad_Name_For_Root_Divinitation>();
            } catch(XObjectContractException e) {
                Assert.AreEqual("Type name of 'Bad_Name_For_Root_Divinitation' cannot contain an underscore if no XObjectRootAttribute was specified", e.Message);
                return;
            }
            Assert.Fail("didn't throw contract exception");
        }

        [XObjectRoot("good-root")]
        public interface I_UnderscoresAreFineWithRootAttributeSpecified : IXObject {
            string Foo { get; }
        }

        [Test]
        public void New_uses_XObjectRoot_attribute() {
            I_UnderscoresAreFineWithRootAttributeSpecified x = XObjectBuilder.New<I_UnderscoresAreFineWithRootAttributeSpecified>();
            Assert.AreEqual("<good-root />", x.AsDocument.ToString());
        }

        [XObjectRoot("x")]
        public interface ICollectionAccessorByConvention : IXObject
        {
            string[] Value { get; set; }
        }

        [Test]
        public void Naming_convention_should_work_for_collection_accessors()
        {
            ICollectionAccessorByConvention x = XObjectBuilder.New<ICollectionAccessorByConvention>();
            x.Value = new string[] {"a","b"};
            Assert.AreEqual("<x><value>a</value><value>b</value></x>",x.AsDocument.ToString());
        }
    }
}
