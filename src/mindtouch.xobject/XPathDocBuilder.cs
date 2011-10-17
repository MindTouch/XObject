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
using System.Text.RegularExpressions;
using MindTouch.Xml;

namespace MindTouch.Dream.XObject {
    public class XPathDocBuilder : IXPathDocBuilder {
        private readonly Regex INVALID_CHARS = new Regex(@"[\[\]()+;'""_=*]", RegexOptions.Compiled);

        public virtual XDoc BuildXDoc(XDoc rootDoc, string xpath, int nodeCount) {
            XDoc subdoc = rootDoc[xpath];
            if(nodeCount == subdoc.ListLength) {
                return subdoc;
            }
            string[] segments = xpath.Split('/');

            // assume the doc root is out rootPath for the collection
            if(nodeCount > 1 && segments.Length == 1 && segments[0].StartsWith("@")) {
                throw new ArgumentException("Cannot have a collection whose xpath is an attribute on the root");
            }
            bool lastIsAttribute = segments[segments.Length - 1].StartsWith("@");
            string last = segments[segments.Length - 1];
            if(lastIsAttribute) {
                last = last.Substring(1);
            }
            string lastElement = null;
            int lastElementIndex = lastIsAttribute ? segments.Length - 2 : segments.Length - 1;
            if(lastElementIndex >= 0) {
                lastElement = segments[lastElementIndex];
            }
            int rootLength = lastIsAttribute ? segments.Length - 1 : segments.Length;
            string[] rootPathSegments = new string[rootLength];
            Array.Copy(segments, rootPathSegments, rootLength);
            XDoc localRoot = CreateRootPath(rootDoc, rootPathSegments);

            if(nodeCount == 1) {
                if(lastIsAttribute && lastElement != null) {
                    if(localRoot[lastElement].IsEmpty) {
                        localRoot.Elem(lastElement);
                    }
                    localRoot = localRoot[lastElement];
                }
                if(lastIsAttribute) {
                    localRoot.Attr(last, "");
                } else {
                    localRoot.Elem(last);
                }
            } else {

                // dump current matches
                foreach(XDoc old in localRoot[lastElement]) {
                    old.Remove();
                }

                // rebuild a fresh set
                if(lastIsAttribute && lastElement != null) {
                    for(int i = 0; i < nodeCount; i++) {
                        localRoot.Start(lastElement).Attr(last,"").End();
                    }
                } else {
                    for(int i = 0; i < nodeCount; i++) {
                        localRoot.Elem(last);
                    }
                }
            }
            subdoc = rootDoc[xpath];
            return subdoc;
        }

        private XDoc CreateRootPath(XDoc rootDoc, string[] segments) {
            string intermediatePath = null;
            XDoc intermediate = rootDoc;
            for(int i = 0; i < segments.Length - 1; i++) {
                if(intermediatePath == null) {
                    intermediatePath = segments[i];
                } else {
                    intermediatePath += "/" + segments[i];
                }
                XDoc next = rootDoc[intermediatePath];
                if(next.IsEmpty) {
                    intermediate.Elem(segments[i]);
                    intermediate = rootDoc[intermediatePath];
                } else {
                    intermediate = next;
                }
            }
            return intermediatePath == null ? rootDoc : rootDoc[intermediatePath];
        }

        public bool CanHandle(string xpath, bool isNodeCollection) {
            return !INVALID_CHARS.IsMatch(xpath);
        }
    }

}
