namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using System.Globalization;

/// <summary>How screens and exports show a statement of work allocation.</summary>
public sealed record StatementOfWorkAllocationView(string Code, string Name, string Ceiling, string Revision, string Capacity, string Rating, string Pinned, string ExpiresAt, string StatusLabel)
{
    public static StatementOfWorkAllocationView ToStatementOfWorkAllocationView(StatementOfWorkAllocation item)
    {
        return new StatementOfWorkAllocationView(
            item.Code,
            item.Name,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkAllocationView> ToStatementOfWorkAllocationViews(List<StatementOfWorkAllocation> items)
    {
        var views = new List<StatementOfWorkAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkAllocationView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkAllocationView(StatementOfWorkAllocationView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedRevision + (double)parsedCapacity;
    }
}
