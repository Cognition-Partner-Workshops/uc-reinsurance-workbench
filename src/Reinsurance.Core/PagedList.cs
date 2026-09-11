using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reinsurance.Core
{
    public interface IPagedList<T> : IList<T>, IPageable
    {
    }

    public class PagedList<T> : List<T>, IPagedList<T>
    {
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
            : base(source ?? Enumerable.Empty<T>())
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int PageNumber { get { return PageIndex + 1; } }
        public int TotalPages { get { return PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize); } }
        public bool HasPreviousPage { get { return PageIndex > 0; } }
        public bool HasNextPage { get { return PageIndex + 1 < TotalPages; } }
    }
}
