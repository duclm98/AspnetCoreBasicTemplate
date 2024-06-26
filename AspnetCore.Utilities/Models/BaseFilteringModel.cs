namespace AspnetCore.Utilities.Models;

public class BaseFilteringModel
{
    private const int MaxPageCount = 100;
    private int _pageCount = MaxPageCount;

    public int Page { get; set; } = 1;

    public int PageCount
    {
        get => _pageCount;
        set => _pageCount = value > MaxPageCount ? MaxPageCount : value;
    }

    public int Skip()
    {
        return PageCount * (Page - 1);
    }
}