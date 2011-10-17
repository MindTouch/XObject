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
using LinFu.DynamicProxy;
using MindTouch.Xml;

namespace MindTouch.Dream.XObject {

    public class XObjectInterceptor : IInterceptor {

        // TODO (arnec): There is quite a bit of efficiency to be gained from dirty tracking of backing fields
        private readonly Type _type;
        private readonly XDoc _doc;
        private readonly Dictionary<string, InterceptionRecord> _memberLookup = new Dictionary<string, InterceptionRecord>();
        private readonly Dictionary<string, object> _backingFields = new Dictionary<string, object>();
        public XObjectInterceptor(Type type, Dictionary<string, InterceptionRecord> memberLookup, XDoc doc) {
            _type = type;
            _memberLookup = memberLookup;
            _doc = doc;
        }

        public object Intercept(InvocationInfo info) {
            if(_doc == null) {
                throw new InvalidOperationException("Cannot use an XObjectInterceptor without an XDoc");
            }
            string methodName = info.TargetMethod.Name;
            if(methodName == "get_AsDocument") {
                RebuildDoc();
                return _doc;
            }
            if(methodName == "get_Item") {
                RebuildDoc();
                return _doc[info.Arguments[0].ToString()];
            }
            string accessor = methodName.Substring(4);
            InterceptionRecord record = _memberLookup[accessor];
            XDoc subdoc = _doc[record.XPath];
            object result;
            _backingFields.TryGetValue(accessor, out result);
            if(result == null) {
                result = record.Convert(subdoc);
                _backingFields[accessor] = result;
            }
            if(methodName.StartsWith("get_")) {
                return result;
            }
            _backingFields[accessor] = info.Arguments[0] ?? string.Empty;
            return null;
        }

        private void RebuildDoc() {
            List<string> keysToWipe = new List<string>();
            foreach(KeyValuePair<string, object> field in _backingFields) {
                InterceptionRecord record = _memberLookup[field.Key];
                if(record.ConvertBack == null || field.Value == null) {
                    continue;
                }
                XDoc subdoc = record.BuildXPath(_doc, record.XPath, record.NodeCount(field.Value));
                record.ConvertBack(subdoc, field.Value);
                keysToWipe.Add(field.Key);
            }
            foreach(string key in keysToWipe) {
                _backingFields.Remove(key);
            }
        }
    }
}
