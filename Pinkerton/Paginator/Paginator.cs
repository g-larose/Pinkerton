using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Paginator
{
    public class Paginator<T>
    {
        public List<List<T>> Paginate(List<T> items, int pageSize)
        {
            List<List<T>> pages = new();

            for (int i = 0; i < items.Count; i += pageSize)
            {
                List<T> page = items.Skip(i).Take(pageSize).ToList();
                pages.Add(page);
            }

            return pages;
        }
    }
}
