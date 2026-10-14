namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using System.Globalization;

/// <summary>How screens and exports show a rolling statement of work allocation.</summary>
public sealed record RollingStatementOfWorkAllocationView(string Code, string Title, string RatioText, string Discount, string Revision, string Ceiling, string Minimum, string StatusLabel)
{
    public static RollingStatementOfWorkAllocationView ToRollingStatementOfWorkAllocationView(RollingStatementOfWorkAllocation item)
    {
        return new RollingStatementOfWorkAllocationView(
            item.Code,
            item.Title,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingStatementOfWorkAllocationView> ToRollingStatementOfWorkAllocationViews(List<RollingStatementOfWorkAllocation> items)
    {
        var views = new List<RollingStatementOfWorkAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToRollingStatementOfWorkAllocationView(item));
        }
        return views;
    }

    public static double SumRollingStatementOfWorkAllocationView(RollingStatementOfWorkAllocationView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedRatio + parsedDiscount + (double)parsedRevision;
    }
}
