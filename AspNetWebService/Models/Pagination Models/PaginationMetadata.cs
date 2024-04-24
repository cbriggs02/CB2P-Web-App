namespace AspNetWebService.Models.Data_Transfer_Object_Models
{
    /// <summary>
    ///     Represents metadata for pagination, including total count, page size, current page, and total pages.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class PaginationMetadata
    {
        /// <summary>
        ///     Gets or sets the total count of items.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        ///     Gets or sets the size of each page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        ///     Gets or sets the current page number.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        ///     Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }
    }
}
