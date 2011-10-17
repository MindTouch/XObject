using System;
using System.Collections.Generic;
using System.Text;

namespace MindTouch.Dream.XObject.Tests.Mocks {

    public class MockXObjectBuilderRepository : IXObjectBuilderRepository {
        public int docBuilderCollectionCalled;
        public IXPathDocBuilder[] docBuilders = new IXPathDocBuilder[0];
   
        public int convertersCollectionCalled;
        public IXObjectTypeConverter[] converters = new IXObjectTypeConverter[0];
        
        public object builder;
        public int getCalled;
        
        public void RegisterDocBuilder(IXPathDocBuilder docBuilder)
        {
            throw new System.NotImplementedException();
        }

        public void RegisterConverter(IXObjectTypeConverter typeConverter)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IXPathDocBuilder> DocBuilders
        {
            get { docBuilderCollectionCalled++; return docBuilders ; }
        }

        public IEnumerable<IXObjectTypeConverter> TypeConverters
        {
            get { convertersCollectionCalled++; return converters; }
        }

        public XObjectBuilder<T> GetBuilder<T>() where T : IXObject
        {
            throw new System.NotImplementedException();
        }

        public object GetBuilder(Type t)
        {
            getCalled++;
            return builder;
        }
    }
}
