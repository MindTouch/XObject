using System;
using System.Collections.Generic;
using System.Text;
using MindTouch.Dream.XObject.Tests.Mocks;
using NUnit.Framework;

namespace MindTouch.Dream.XObject.Tests {

    // TODO: need test for passed in converter to an XObjectBuilder
    // TODO: need test for passed in doc builder
    [TestFixture]
    public class XObjectBuilderTests {

        public interface ITestXObject : IXObject {
            string Foo { get; }
        }

        public interface ISuperTestXObject : IXObject {
            ITestXObject Sub { get; }
        }

        [Test]
        public void XObjectBuilder_accesses_doc_and_converter_collections() {
            MockXObjectBuilderRepository repository = new MockXObjectBuilderRepository();
            XObjectBuilder<ITestXObject> builder = new XObjectBuilder<ITestXObject>(repository);
            Assert.AreEqual(1, repository.convertersCollectionCalled);
            Assert.AreEqual(1, repository.docBuilderCollectionCalled);
        }

        [Test]
        public void XObjectBuilder_with_IXObject_accessor_accesses_repository_GetBuilder()
        {
            MockXObjectBuilderRepository repository = new MockXObjectBuilderRepository();
            XObjectBuilder<ITestXObject> subBuilder = new XObjectBuilder<ITestXObject>(repository);
            repository.convertersCollectionCalled = 0;
            repository.docBuilderCollectionCalled = 0;
            repository.builder = subBuilder;
            XObjectBuilder<ISuperTestXObject> superBuilder = new XObjectBuilder<ISuperTestXObject>(repository);
            Assert.AreEqual(1, repository.convertersCollectionCalled);
            Assert.AreEqual(1, repository.docBuilderCollectionCalled);
            Assert.AreEqual(1, repository.getCalled);
        }
    }
}
