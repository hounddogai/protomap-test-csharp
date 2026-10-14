namespace Acme.Workforce.Absence.Accrual.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual assignment.</summary>
public sealed record AccrualAssignmentView(string Code, string Title, string GradeText, string EndsAt, string Checksum, string Limit, string StatusLabel)
{
    public static AccrualAssignmentView ToAccrualAssignmentView(AccrualAssignment item)
    {
        return new AccrualAssignmentView(
            Text.Truncate(item.Code, 64),
            item.Title,
            Text.Truncate(item.Grade, 80),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 64),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualAssignmentView> ToAccrualAssignmentViews(List<AccrualAssignment> items)
    {
        var views = new List<AccrualAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualAssignmentView(item));
        }
        return views;
    }

    public static double SumAccrualAssignmentView(AccrualAssignmentView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedLimit;
    }
}
