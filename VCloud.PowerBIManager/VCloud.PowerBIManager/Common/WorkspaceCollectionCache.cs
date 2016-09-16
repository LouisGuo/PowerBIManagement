using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCloud.PowerBIManager
{
    public static class WorkspaceCollectionCache
    {
        public static readonly List<WorkspaceCollection> Cache;

        static WorkspaceCollectionCache()
        {
            Cache = FileHelper.GetHistoryWorkspaceCollections();
        }

        public static void AddOrUpdate(WorkspaceCollection collection)
        {
            if (collection != null)
            {
                var index = Cache.FindIndex(c => c.Name.Equals(collection.Name, StringComparison.OrdinalIgnoreCase));
                if (index > -1)
                    Cache[index] = collection;
                else
                    Cache.Insert(0, collection);
                UpdateHistoryFile();
            }
        }

        public static void AddRange(List<WorkspaceCollection> collections)
        {
            collections = collections.Where(c => Cache.
              FirstOrDefault(all => all.Name.Equals(c.Name, StringComparison.OrdinalIgnoreCase)) == null).ToList();
            Cache.AddRange(collections);
            UpdateHistoryFile();
        }

        public static void InsertRange(int index, List<WorkspaceCollection> collections)
        {
            collections = collections.Where(c => Cache.
              FirstOrDefault(all => all.Name.Equals(c.Name, StringComparison.OrdinalIgnoreCase)) == null).ToList();
            Cache.InsertRange(index, collections);
            UpdateHistoryFile();
        }

        private static void UpdateHistoryFile()
        {
            FileHelper.UpdateHistoryWorkspaceCollections(Cache);
        }
    }
}
