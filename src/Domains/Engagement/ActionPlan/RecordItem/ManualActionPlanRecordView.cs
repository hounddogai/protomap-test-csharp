namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual action plan record.</summary>
public sealed record ManualActionPlanRecordView(string Code, string Title, string Version, string CategoryText, string Discount, string Hours, string Name, string ExpiresAt, string LeaveDaysText, string StatusLabel)
{
    public static ManualActionPlanRecordView ToManualActionPlanRecordView(ManualActionPlanRecord item)
    {
        return new ManualActionPlanRecordView(
            item.Code,
            item.Title,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 40),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualActionPlanRecordView> ToManualActionPlanRecordViews(List<ManualActionPlanRecord> items)
    {
        var views = new List<ManualActionPlanRecordView>();
        foreach (var item in items)
        {
            views.Add(ToManualActionPlanRecordView(item));
        }
        return views;
    }

    public static double SumManualActionPlanRecordView(ManualActionPlanRecordView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedDiscount + parsedHours;
    }
}
