using System;
using System.Collections.Generic;
using System.Text;
using MindTouch.Xml;
using NUnit.Framework;

namespace MindTouch.Dream.XObject.Tests {
    [TestFixture]
    public class XPathDocBuilderTester {

        private readonly XPathDocBuilder _converter = new XPathDocBuilder();

        [Test]
        public void XPath_into_attribute_found_in_list_of_elemnts_returns_list_of_attributes() {
            XDoc doc = new XDoc("doc")
                .Start("x").Attr("b", 1).End()
                .Start("x").Attr("b", 2).End()
                .Start("x").Attr("b", 3).End();
            XDoc b = doc["x/@b"];
            Assert.AreEqual(3, b.ListLength);
        }

        [Test]
        public void ListLength_of_empty_XDoc_is_zero() {
            XDoc doc = new XDoc("doc");
            XDoc x = doc["x"];
            Assert.AreEqual(0, x.ListLength);
        }

        [Test]
        public void Refuses_special_chars() {
            Assert.IsFalse(_converter.CanHandle("[", false), "Accepted [");
            Assert.IsFalse(_converter.CanHandle("]", false), "Accepted ]");
            Assert.IsFalse(_converter.CanHandle("\"", false), "Accepted \"");
            Assert.IsFalse(_converter.CanHandle("_", false), "Accepted _");
            Assert.IsFalse(_converter.CanHandle("'", false), "Accepted '");
            Assert.IsFalse(_converter.CanHandle("(", false), "Accepted (");
            Assert.IsFalse(_converter.CanHandle(")", false), "Accepted )");
            Assert.IsFalse(_converter.CanHandle("+", false), "Accepted +");
            Assert.IsFalse(_converter.CanHandle("=", false), "Accepted =");
            Assert.IsFalse(_converter.CanHandle(";", false), "Accepted ;");
            Assert.IsFalse(_converter.CanHandle("*", false), "Accepted ;");
        }

        [Test]
        public void Accepts_valid_chars() {
            Assert.IsTrue(_converter.CanHandle("foo", false), "Rejected foo");
            Assert.IsTrue(_converter.CanHandle("@foo", false), "Rejected @foo");
            Assert.IsTrue(_converter.CanHandle("/foo/bar", false), "Rejected /foo/bar");
            Assert.IsTrue(_converter.CanHandle("foo/bar", false), "Rejected foo/bar");
            Assert.IsTrue(_converter.CanHandle("foo-bar", false), "Rejected foo-bar");
            Assert.IsTrue(_converter.CanHandle("foo:bar", false), "Rejected foo:bar");
        }

        [Test]
        public void Builds_element() {
            XDoc x = new XDoc("doc");
            string xpath = "foo";
            Assert.IsTrue(x[xpath].IsEmpty);
            _converter.BuildXDoc(x, xpath, 1);
            Assert.IsFalse(x[xpath].IsEmpty);
            Assert.AreEqual("<doc><foo /></doc>", x.ToString());
        }

        [Test]
        public void Builds_attribute() {
            XDoc x = new XDoc("doc");
            string xpath = "@foo";
            Assert.IsTrue(x[xpath].IsEmpty);
            _converter.BuildXDoc(x, xpath, 1);
            Assert.IsFalse(x[xpath].IsEmpty);
            Assert.AreEqual("<doc foo=\"\" />", x.ToString());
        }

        [Test]
        public void Builds_simple_elem_path() {
            XDoc x = new XDoc("doc");
            string xpath = "foo/bar/baz";
            Assert.IsTrue(x[xpath].IsEmpty);
            _converter.BuildXDoc(x, xpath, 1);
            Assert.IsFalse(x[xpath].IsEmpty, x.ToString());
            Assert.AreEqual("<doc><foo><bar><baz /></bar></foo></doc>", x.ToString());
        }

        [Test]
        public void Builds_simple_elem_path_with_attr_at_end() {
            XDoc x = new XDoc("doc");
            string xpath = "foo/bar/@baz";
            Assert.IsTrue(x[xpath].IsEmpty);
            _converter.BuildXDoc(x, xpath, 1);
            Assert.IsFalse(x[xpath].IsEmpty, x.ToString());
            Assert.AreEqual("<doc><foo><bar baz=\"\" /></foo></doc>", x.ToString());
        }

        [Test]
        public void Builds_simple_elem_path_with_partial_path_in_existence() {
            XDoc x = new XDoc("doc").Elem("foo");
            string xpath = "foo/bar/baz";
            Assert.IsTrue(x[xpath].IsEmpty);
            _converter.BuildXDoc(x, xpath, 1);
            Assert.IsFalse(x[xpath].IsEmpty, x.ToString());
            Assert.AreEqual("<doc><foo><bar><baz /></bar></foo></doc>", x.ToString());
        }

        [Test]
        public void Adds_Attribute_to_existing_element() {
            XDoc x = new XDoc("doc").Elem("foo");
            string xpath = "foo/bar/@baz";
            Assert.IsTrue(x[xpath].IsEmpty);
            _converter.BuildXDoc(x, xpath, 1);
            Assert.IsFalse(x[xpath].IsEmpty, x.ToString());
            Assert.AreEqual("<doc><foo><bar baz=\"\" /></foo></doc>", x.ToString());
        }

        [Test]
        public void NodeCount_greater_than_one_on_a_root_attribute_throws() {
            try {
                _converter.BuildXDoc(new XDoc("foo"), "@id", 2);
            } catch(ArgumentException e) {
                Assert.AreEqual("Cannot have a collection whose xpath is an attribute on the root", e.Message);
                return;
            }
            Assert.Fail("didn't catch expeced exception");
        }

        [Test]
        public void Builds_array_of_elem_on_root() {
            XDoc x = new XDoc("doc");
            _converter.BuildXDoc(x, "foo", 5);
            Assert.AreEqual(5, x["foo"].ListLength);
        }

        [Test]
        public void Add_more_elem_to_get_to_expected_count_on_root() {
            XDoc x = new XDoc("doc").Elem("foo");
            Assert.AreEqual(1, x["foo"].ListLength);
            _converter.BuildXDoc(x, "foo", 5);
            Assert.AreEqual(5, x["foo"].ListLength);
        }

        [Test]
        public void Removes_elem_to_get_to_expected_count_on_root() {
            XDoc x = new XDoc("doc").Elem("foo").Elem("foo").Elem("foo").Elem("foo");
            Assert.AreEqual(4, x["foo"].ListLength);
            _converter.BuildXDoc(x, "foo", 2);
            Assert.AreEqual(2, x["foo"].ListLength);
        }

        [Test]
        public void Builds_array_of_elem_with_attr_on_root() {
            XDoc x = new XDoc("doc");
            _converter.BuildXDoc(x, "foo/@bar", 5);
            Assert.AreEqual(5, x["foo/@bar"].ListLength);
        }

        [Test]
        public void Add_more_elem_with_attr_to_get_to_expected_count_on_root() {
            XDoc x = new XDoc("doc")
                .Start("foo")
                    .Attr("bar", "")
                .End();
            Assert.AreEqual(1, x["foo/@bar"].ListLength);
            _converter.BuildXDoc(x, "foo/@bar", 5);
            Assert.AreEqual(5, x["foo/@bar"].ListLength);
        }

        [Test]
        public void Removes_elem_with_attr_to_get_to_expected_count_on_root() {
            XDoc x = new XDoc("doc")
                .Start("foo")
                    .Attr("bar", "")
                .End()
                .Start("foo")
                    .Attr("bar", "")
                .End()
                .Start("foo")
                    .Attr("bar", "")
                .End()
                .Start("foo")
                    .Attr("bar", "")
                .End();
            Assert.AreEqual(4, x["foo/@bar"].ListLength);
            _converter.BuildXDoc(x, "foo/@bar", 2);
            Assert.AreEqual(2, x["foo/@bar"].ListLength);
        }

        [Test]
        public void Add_more_elem_with_attr_to_get_to_expected_count_on_root_even_if_partial_exists() {
            XDoc x = new XDoc("doc")
                .Elem("foo");
            Assert.AreEqual(1, x["foo"].ListLength);
            _converter.BuildXDoc(x, "foo/@bar", 2);
            Assert.AreEqual(2, x["foo/@bar"].ListLength);
            Assert.AreEqual("<doc><foo bar=\"\" /><foo bar=\"\" /></doc>", x.ToString());
        }

        [Test]
        public void Builds_array_of_element_paths() {
            XDoc x = new XDoc("doc");
            _converter.BuildXDoc(x, "foo/bar", 5);
            Assert.AreEqual(5, x["foo/bar"].ListLength);
        }

        [Test]
        public void Add_more_elements_at_end_of_path_to_get_to_expected_count() {
            XDoc x = new XDoc("doc")
                .Start("foo")
                    .Elem("bar")
                .End();
            Assert.AreEqual(1, x["foo/bar"].ListLength);
            _converter.BuildXDoc(x, "foo/bar", 5);
            Assert.AreEqual(5, x["foo/bar"].ListLength);
        }

        [Test]
        public void Removes_elements_at_end_of_path_to_get_to_expected_count() {
            XDoc x = new XDoc("doc")
                .Start("foo")
                    .Elem("bar")
                    .Elem("bar")
                    .Elem("bar")
                    .Elem("bar")
                .End();
            Assert.AreEqual(4, x["foo/bar"].ListLength);
            _converter.BuildXDoc(x, "foo/bar", 2);
            Assert.AreEqual(2, x["foo/bar"].ListLength);
        }

        [Test]
        public void Add_more_elements_at_end_of_path_to_get_to_expected_count_even_if_partial_exists() {
            XDoc x = new XDoc("doc")
                .Elem("foo");
            Assert.AreEqual(1, x["foo"].ListLength);
            _converter.BuildXDoc(x, "foo/bar/baz", 2);
            Assert.AreEqual(2, x["foo/bar/baz"].ListLength);
            Assert.AreEqual("<doc><foo><bar><baz /><baz /></bar></foo></doc>", x.ToString());
        }

        [Test]
        public void Builds_array_of_elements_with_attr_paths() {
            XDoc x = new XDoc("doc");
            _converter.BuildXDoc(x, "foo/bar/@baz", 5);
            Assert.AreEqual(5, x["foo/bar/@baz"].ListLength);
        }

        [Test]
        public void Add_more_elements_with_attr_at_end_of_path_to_get_to_expected_count() {
            XDoc x = new XDoc("doc")
                .Start("foo")
                    .Start("bar")
                        .Attr("baz","")
                    .End()
                .End();
            Assert.AreEqual(1, x["foo/bar/@baz"].ListLength);
            _converter.BuildXDoc(x, "foo/bar/@baz", 5);
            Assert.AreEqual(5, x["foo/bar/@baz"].ListLength);
        }

        [Test]
        public void Removes_elements_with_attr_at_end_of_path_to_get_to_expected_count() {
            XDoc x = new XDoc("doc")
                .Start("foo")
                    .Start("bar")
                        .Attr("baz", "")
                    .End()
                    .Start("bar")
                        .Attr("baz", "")
                    .End()
                    .Start("bar")
                        .Attr("baz", "")
                    .End()
                    .Start("bar")
                        .Attr("baz", "")
                    .End()
                .End();
            Assert.AreEqual(4, x["foo/bar/@baz"].ListLength);
            _converter.BuildXDoc(x, "foo/bar/@baz", 2);
            Assert.AreEqual(2, x["foo/bar/@baz"].ListLength);
        }

        [Test]
        public void Add_more_elements_with_attr_at_end_of_path_to_get_to_expected_count_even_if_partial_exists() {
            XDoc x = new XDoc("doc")
                .Start("foo").Elem("bar").End();
            Assert.AreEqual(1, x["foo"].ListLength);
            _converter.BuildXDoc(x, "foo/bar/@baz", 2);
            Assert.AreEqual(2, x["foo/bar/@baz"].ListLength);
            Assert.AreEqual("<doc><foo><bar baz=\"\" /><bar baz=\"\" /></foo></doc>", x.ToString());
        }
    }
}
