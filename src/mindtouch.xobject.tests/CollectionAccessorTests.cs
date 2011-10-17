using System.Collections.Generic;
using MindTouch.Xml;
using NUnit.Framework;

namespace MindTouch.Dream.XObject.Tests {

    [TestFixture]
    public class CollectionAccessorTests {

        public interface IArrayMemberTest : IXObject {
            [XObjectPath("users/user/@id")]
            int[] UserIds { get; set; }
        }

        [Test]
        public void Array_of_x_can_convert_node_list_from_xpath() {
            XDoc doc = new XDoc("doc")
                .Start("users")
                    .Start("user").Attr("id", 1).End()
                    .Start("user").Attr("id", 2).End()
                    .Start("user").Attr("id", 3).End()
                .End();
            IArrayMemberTest x = XObjectBuilder.FromXDoc<IArrayMemberTest>(doc);
            Assert.AreEqual(3, x.UserIds.Length);
            Assert.AreEqual(1, x.UserIds[0]);
            Assert.AreEqual(2, x.UserIds[1]);
            Assert.AreEqual(3, x.UserIds[2]);
        }

        [Test]
        public void Can_set_array_on_XObject() {
            IArrayMemberTest x = XObjectBuilder.New<IArrayMemberTest>();
            x.UserIds = new int[] { 6, 7 };
            Assert.AreEqual("<array-member-test><users><user id=\"6\" /><user id=\"7\" /></users></array-member-test>", x.AsDocument.ToString());
        }

        [Test]
        public void Can_set_element_on_array_on_XObject() {
            XDoc doc = new XDoc("doc")
                .Start("users")
                    .Start("user").Attr("id", 1).End()
                    .Start("user").Attr("id", 2).End()
                .End();
            IArrayMemberTest x = XObjectBuilder.FromXDoc<IArrayMemberTest>(doc);
            x.UserIds[1] = 5;
            Assert.AreEqual(5, x.UserIds[1]);
            Assert.AreEqual("<doc><users><user id=\"1\" /><user id=\"5\" /></users></doc>", x.AsDocument.ToString());
        }

        public interface IUserTest : IXObject {
            int id { get; set;}
            string Name { get; set;}
        }

        [XObjectRoot("x")]
        public interface IArrayOfUsersTest : IXObject {
            [XObjectPath("users/user")]
            IUserTest[] Users { get; set;}
        }

        [Test]
        public void Can_set_array_of_XObject_on_XObject() {
            IArrayOfUsersTest x = XObjectBuilder.New<IArrayOfUsersTest>();
            x.Users = new IUserTest[] {
                XObjectBuilder.New<IUserTest>(),
                XObjectBuilder.New<IUserTest>()
            };
            x.Users[0].id = 1;
            x.Users[0].Name = "bob";
            x.Users[1].id = 2;
            x.Users[1].Name = "jane";
            Assert.AreEqual("<x><users><user id=\"1\"><name>bob</name></user><user id=\"2\"><name>jane</name></user></users></x>", x.AsDocument.ToString());
        }

        [Test]
        public void Can_set_XObject_element_in_array_on_XObject() {
            XDoc doc = new XDoc("x")
                .Start("users")
                    .Start("user").Attr("id", 1).Elem("name", "bob").End()
                    .Start("user").Attr("id", 2).Elem("name", "jane").End()
                .End();
            IArrayOfUsersTest x = XObjectBuilder.FromXDoc<IArrayOfUsersTest>(doc);
            IUserTest user = XObjectBuilder.New<IUserTest>();
            user.id = 5;
            user.Name = "jack";
            x.Users[1] = user;
            Assert.AreEqual("<x><users><user id=\"1\"><name>bob</name></user><user id=\"5\"><name>jack</name></user></users></x>", x.AsDocument.ToString());
        }

        [XObjectRoot("x")]
        public interface IListMemberTest : IXObject {
            [XObjectPath("users/user/@id")]
            IList<int> UserIds { get; }
        }

        [Test]
        public void List_of_x_can_convert_node_list_from_xpath() {
            XDoc doc = new XDoc("doc")
                .Start("users")
                    .Start("user").Attr("id", 1).End()
                    .Start("user").Attr("id", 2).End()
                    .Start("user").Attr("id", 3).End()
                .End();
            IListMemberTest x = XObjectBuilder.FromXDoc<IListMemberTest>(doc);
            Assert.AreEqual(3, x.UserIds.Count);
            Assert.AreEqual(1, x.UserIds[0]);
            Assert.AreEqual(2, x.UserIds[1]);
            Assert.AreEqual(3, x.UserIds[2]);
        }

        [Test]
        public void Can_add_to_IList() {
            IListMemberTest x = XObjectBuilder.New<IListMemberTest>();
            x.UserIds.Add(5);
            x.UserIds.Add(7);
            x.UserIds.Add(6);
            Assert.AreEqual("<x><users><user id=\"5\" /><user id=\"7\" /><user id=\"6\" /></users></x>", x.AsDocument.ToString());
        }

        [Test]
        public void Can_remove_from_IList() {
            XDoc doc = new XDoc("x")
                .Start("users")
                    .Start("user").Attr("id", 1).End()
                    .Start("user").Attr("id", 2).End()
                    .Start("user").Attr("id", 3).End()
                .End();
            IListMemberTest x = XObjectBuilder.FromXDoc<IListMemberTest>(doc);
            x.UserIds.RemoveAt(1);
            Assert.AreEqual("<x><users><user id=\"1\" /><user id=\"3\" /></users></x>", x.AsDocument.ToString());
        }

        [Test]
        public void Can_access_IList_via_indexer() {
            XDoc doc = new XDoc("x")
                .Start("users")
                    .Start("user").Attr("id", 1).End()
                    .Start("user").Attr("id", 2).End()
                    .Start("user").Attr("id", 3).End()
                .End();
            IListMemberTest x = XObjectBuilder.FromXDoc<IListMemberTest>(doc);
            Assert.AreEqual(2, x.UserIds[1]);
        }

        [Test]
        public void Can_clear_IList() {
            XDoc doc = new XDoc("x")
                .Start("users")
                    .Start("user").Attr("id", 1).End()
                    .Start("user").Attr("id", 2).End()
                    .Start("user").Attr("id", 3).End()
                .End();
            IListMemberTest x = XObjectBuilder.FromXDoc<IListMemberTest>(doc);
            x.UserIds.Clear();
            Assert.AreEqual("<x><users></users></x>", x.AsDocument.ToString());
        }

        public interface IEnumerableMemberTest : IXObject {
            [XObjectPath("users/user/@id")]
            IEnumerable<int> UserIds { get; }
        }

        [Test]
        public void IEnumerable_of_x_can_convert_node_list_from_xpath() {
            XDoc doc = new XDoc("doc")
                .Start("users")
                    .Start("user").Attr("id", 1).End()
                    .Start("user").Attr("id", 2).End()
                    .Start("user").Attr("id", 3).End()
                .End();
            IEnumerableMemberTest x = XObjectBuilder.FromXDoc<IEnumerableMemberTest>(doc);
            List<int> list = new List<int>();
            list.AddRange(x.UserIds);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
        }

        public interface ICollectionMemberTest : IXObject {
            [XObjectPath("users/user/@id")]
            ICollection<int> UserIds { get; }
        }

        [Test]
        public void ICollection_of_x_can_convert_node_list_from_xpath() {
            XDoc doc = new XDoc("doc")
                .Start("users")
                    .Start("user").Attr("id", 1).End()
                    .Start("user").Attr("id", 2).End()
                    .Start("user").Attr("id", 3).End()
                .End();
            ICollectionMemberTest x = XObjectBuilder.FromXDoc<ICollectionMemberTest>(doc);
            Assert.AreEqual(3, x.UserIds.Count);
            List<int> list = new List<int>();
            list.AddRange(x.UserIds);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
        }
    }
}
