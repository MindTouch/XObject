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
using MindTouch.Xml;

namespace MindTouch.Dream.XObject.TypeConverter {
    public class XObjectEnumConverter<T> : IXObjectTypeConverter {
        private readonly T @default;
        private readonly Type enumType;

        public XObjectEnumConverter() {
            Type t = typeof(T);
            if(t.IsEnum) {
                enumType = t;
                return;
            }
            if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>) && t.GetGenericArguments()[0].IsEnum) {
                enumType = t.GetGenericArguments()[0];
                return;
            }
            throw new ArgumentException("Generic parameter must be an enum or nullable enum");
        }

        public XObjectEnumConverter(T @default)
            : this() {
            this.@default = @default;
        }

        public bool CanConvert(Type t)
        {
            return enumType.IsAssignableFrom(t);
            ;
        }

        public object Convert(XDoc doc) {
            return doc.IsEmpty ? @default : Enum.Parse(enumType, doc.AsText, true);
        }

        public void ConvertBack(XDoc doc, object value)
        {
            throw new System.NotImplementedException();
        }

        public bool CanConvertBack { get { return true; } }

    }
}
