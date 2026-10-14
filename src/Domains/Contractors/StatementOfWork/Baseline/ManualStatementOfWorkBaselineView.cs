namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual statement of work baseline.</summary>
public sealed record ManualStatementOfWorkBaselineView(string CodeText, string Title, string Reviewer, string MaximumText, string GraceDays, string ContactEmail, string CapacityText, string Unit, string CategoryText, string ManagerId, string DurationMinutes, string StatusLabel)
{
    public static ManualStatementOfWorkBaselineView ToManualStatementOfWorkBaselineView(ManualStatementOfWorkBaseline item)
    {
        return new ManualStatementOfWorkBaselineView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Reviewer,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            Text.Truncate(item.Category, 120),
            item.ManagerId,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualStatementOfWorkBaselineView> ToManualStatementOfWorkBaselineViews(List<ManualStatementOfWorkBaseline> items)
    {
        var views = new List<ManualStatementOfWorkBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToManualStatementOfWorkBaselineView(item));
        }
        return views;
    }

    public static double SumManualStatementOfWorkBaselineView(ManualStatementOfWorkBaselineView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedGraceDays + (double)parsedCapacity;
    }
}
