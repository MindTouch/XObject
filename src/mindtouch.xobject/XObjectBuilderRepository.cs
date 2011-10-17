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
using System.Collections.Generic;

namespace MindTouch.Dream.XObject {
    public class XObjectBuilderRepository : IXObjectBuilderRepository {
        readonly Dictionary<Type, object> _repository = new Dictionary<Type, object>();
        readonly List<IXPathDocBuilder> _docBuilders = new List<IXPathDocBuilder>();
        readonly List<IXObjectTypeConverter> _converters = new List<IXObjectTypeConverter>();

        public void RegisterDocBuilder(IXPathDocBuilder docBuilder) {
            _docBuilders.Add(docBuilder);
        }

        public void RegisterConverter(IXObjectTypeConverter typeConverter) {
            _converters.Add(typeConverter);
        }

        public IEnumerable<IXPathDocBuilder> DocBuilders {
            get { return _docBuilders.ToArray(); }
        }

        public IEnumerable<IXObjectTypeConverter> TypeConverters {
            get { return _converters.ToArray(); }
        }

        public XObjectBuilder<T> GetBuilder<T>() where T : IXObject {
            Type t = typeof(T);

            // The code is duplicated from GetBuilder(Type t), since the generic version can use the faster new instead of building by reflection
            lock(_repository) {
                object builder;
                if(!_repository.TryGetValue(t, out builder)) {
                    XObjectBuilder<T> newBuilder = new XObjectBuilder<T>(this);
                    _repository.Add(t, newBuilder);
                    return newBuilder;
                }
                return builder as XObjectBuilder<T>;
            }
        }

        public object GetBuilder(Type t) {
            if(!typeof(IXObject).IsAssignableFrom(t)) {
                throw new ArgumentException(string.Format("Cannot retrieve builder for non IXObject type '{0}'", t));
            }
            lock(_repository) {
                object builder;
                if(!_repository.TryGetValue(t, out builder)) {
                    Type genericBuilderType = typeof(XObjectBuilder<>);
                    Type builderType = genericBuilderType.MakeGenericType(t);
                    builder = Activator.CreateInstance(builderType, new object[] { this });
                    _repository.Add(t, builder);
                }
                return builder;
            }
        }
    }
}
