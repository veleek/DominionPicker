namespace Ben.Dominion
{
    public enum ResultSortOrder
    {
        None,
        Name,
        Cost,
        Set,
    }

    public static class ResultSortOrderExtensions
    {
        public static ResultSortOrder Next(this ResultSortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case ResultSortOrder.Name:
                    return ResultSortOrder.Cost;
                case ResultSortOrder.Cost:
                    return ResultSortOrder.Set;
                case ResultSortOrder.Set:
                    return ResultSortOrder.Name;
                default:
                    return ResultSortOrder.Name;
            }
        }
    }
}