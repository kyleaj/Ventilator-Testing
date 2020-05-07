using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VentilatorTesting
{
    public class FlushableList<T>
    {
        private class ListItem
        {
            public DateTime timeStamp;
            public T value;

            public override string ToString()
            {
                return $"{timeStamp.Subtract(new DateTime(2000, 1, 1)).TotalMilliseconds}, {value}";
            }
        }


        private string Path;
        private List<ListItem> List;
        private static readonly int MAX_LENGTH = 1000; // Chosen arbitrarily
        private readonly object LockObj = new object();

        public FlushableList(string outputFilePath)
        {
            Path = outputFilePath;
        }

        public void Add(T sample)
        {
            if (List.Count >= MAX_LENGTH)
            {
                var list = List;
                List = new List<ListItem>();
                _ = FlushList();
            }
            List.Add(new ListItem { timeStamp = DateTime.Now, value = sample });
        }

        public async Task FlushList()
        {
            await FlushList(List);
        }

        private async Task FlushList(List<ListItem> list)
        {
            await Task.Run(() =>
            {
                lock (LockObj)
                {
                    File.AppendAllLines(Path, list.Select(a => a.ToString()));
                }
            });
        }
    }
}
