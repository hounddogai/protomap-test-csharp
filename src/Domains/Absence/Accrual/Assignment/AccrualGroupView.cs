namespace Acme.Workforce.Absence.Accrual.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual group.</summary>
public sealed record AccrualGroupView(string Code, string Name, string Revision, string Source, string BatchSize, string Ceiling, string EmployeeIdText, string GradeText, string PriorityText, string StatusLabel)
{
    public static AccrualGroupView ToAccrualGroupView(AccrualGroup item)
    {
        return new AccrualGroupView(
            item.Code,
            item.Name,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            Text.Truncate(item.Grade, 64),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualGroupView> ToAccrualGroupViews(List<AccrualGroup> items)
    {
        var views = new List<AccrualGroupView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualGroupView(item));
        }
        return views;
    }

    public static double SumAccrualGroupView(AccrualGroupView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedBatchSize + parsedCeiling;
    }
}
