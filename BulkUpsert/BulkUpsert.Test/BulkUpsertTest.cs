using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace BulkUpsert.Test
{
    [TestClass]
    public class BulkUpsertTest
    {
        [TestMethod]
        public void RepoTest1()
        {
            // Inital Data
            var data = new List<SomeTable>();
            data.Add(new SomeTable() { field1 = 111, field2 = "111", field3 = false, unique_field = 1 });
            data.Add(new SomeTable() { field1 = 222, field2 = "222", field3 = true, unique_field = 2 });
            data.Add(new SomeTable() { field1 = 333, field2 = "333", field3 = true, unique_field = 3 });

            // Insert data
            var ids = Repository.StoreData(data);
            Assert.AreEqual(data.Count(), ids.Count());

            // Query and verify data
            var retrieved = Repository.GetData(ids);
            Assert.AreEqual(data.Count(), retrieved.Count());
            for(var i = 0; i < data.Count(); i++)
            {
                Assert.IsTrue(data[i].Equals(retrieved[i]));
            }

            // Update data
            var updates = new List<SomeTable>();
            updates.Add(new SomeTable() { field1 = 444, field2 = "444", field3 = true, unique_field = 1 });
            updates.Add(new SomeTable() { field1 = 555, field2 = "555", field3 = false, unique_field = 2 });
            updates.Add(new SomeTable() { field1 = 666, field2 = "666", field3 = false, unique_field = 3 });
            var updated_ids = Repository.StoreData(updates);

            // Query and verify updates
            Assert.AreEqual(updated_ids.Count(), ids.Count());
            for(var i = 0; i < updated_ids.Count(); i++)
            {
                Assert.AreEqual(ids[i], updated_ids[i]);
            }
            var retrieved_updates = Repository.GetData(updated_ids);
            Assert.AreEqual(retrieved_updates.Count(), retrieved.Count());
            for (var i = 0; i < updates.Count(); i++)
            {
                Assert.IsTrue(updates[i].Equals(retrieved_updates[i]));
            }

            // Clean data
            Repository.RemoveData(ids);
        }
    }
}
