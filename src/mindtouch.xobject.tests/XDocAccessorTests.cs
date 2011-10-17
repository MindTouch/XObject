using MindTouch.Xml;
using NUnit.Framework;

namespace MindTouch.Dream.XObject.Tests {

    [TestFixture]
    public class XDocAccessorTests {

        public interface IXDocAccessor : IXObject {
            XDoc Doc { get; set; }
        }

        [Test]
        public void Can_get_XDoc() {
            XDoc x = new XDoc("x").Start("doc").Elem("foo", "bar").End();
            IXDocAccessor xdoc = XObjectBuilder.FromXDoc<IXDocAccessor>(x);
            Assert.AreEqual("<doc><foo>bar</foo></doc>", xdoc.Doc.ToString());
        }

        [Test]
        public void Can_set_XDoc() {
            XDoc x = new XDoc("x").Start("doc").Elem("foo", "bar").End();
            IXDocAccessor xdoc = XObjectBuilder.FromXDoc<IXDocAccessor>(x);
            xdoc.Doc = new XDoc("blah").Attr("a", "b").Elem("floop", "flop");

            // Note (arnec): this is a bit of an unexpected artifact.. the root node does not get renamed until the next XDoc rendering
            Assert.AreEqual("<blah a=\"b\"><floop>flop</floop></blah>", xdoc.Doc.ToString());
            Assert.AreEqual("<x><doc a=\"b\"><floop>flop</floop></doc></x>", xdoc.AsDocument.ToString());
            Assert.AreEqual("<doc a=\"b\"><floop>flop</floop></doc>", xdoc.Doc.ToString());
        }

    }
}
