namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

using System.Globalization;

/// <summary>How screens and exports show a rolling statement of work variance.</summary>
public sealed record RollingStatementOfWorkVarianceView(string Code, string Title, string Pinned, string GradeText, string CapacityText, string DescriptionText, string PriorityText, string Department, string StatusLabel)
{
    public static RollingStatementOfWorkVarianceView ToRollingStatementOfWorkVarianceView(RollingStatementOfWorkVariance item)
    {
        return new RollingStatementOfWorkVarianceView(
            item.Code,
            item.Title,
            $"{item.Pinned}",
            item.Grade,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingStatementOfWorkVarianceView> ToRollingStatementOfWorkVarianceViews(List<RollingStatementOfWorkVariance> items)
    {
        var views = new List<RollingStatementOfWorkVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToRollingStatementOfWorkVarianceView(item));
        }
        return views;
    }

    public static double SumRollingStatementOfWorkVarianceView(RollingStatementOfWorkVarianceView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedPriority;
    }
}
