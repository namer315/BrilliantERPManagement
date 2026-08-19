namespace CommonData.Search;

public class Pagination
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public Pagination()
    {
        PageNumber = 1;
        PageSize = 10;
    }
    public Pagination(int pageNumber , int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber) , "Page number must be greater than 0.");
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize) , "Page size must be greater than 0.");

        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int Offset => (PageNumber - 1) * PageSize;
}

