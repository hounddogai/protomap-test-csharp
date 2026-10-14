namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted statement of work assignment.</summary>
public sealed record WeightedStatementOfWorkAssignmentView(string CodeText, string Label, string ContactEmailText, string TenureMonthsText, string WeightText, string ActiveText, string Locale, string CeilingText, string StatusLabel)
{
    public static WeightedStatementOfWorkAssignmentView ToWeightedStatementOfWorkAssignmentView(WeightedStatementOfWorkAssignment item)
    {
        return new WeightedStatementOfWorkAssignmentView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.ContactEmail, 80),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            Text.Truncate(item.Locale, 80),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedStatementOfWorkAssignmentView> ToWeightedStatementOfWorkAssignmentViews(List<WeightedStatementOfWorkAssignment> items)
    {
        var views = new List<WeightedStatementOfWorkAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedStatementOfWorkAssignmentView(item));
        }
        return views;
    }

    public static double SumWeightedStatementOfWorkAssignmentView(WeightedStatementOfWorkAssignmentView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedWeight + parsedCeiling;
    }
}
