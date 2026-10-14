namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary succession plan record.</summary>
public sealed record PrimarySuccessionPlanRecordView(string Code, string Name, string RegionText, string DurationMinutes, string LeaveDays, string Location, string Weight, string Notes, string StatusLabel)
{
    public static PrimarySuccessionPlanRecordView ToPrimarySuccessionPlanRecordView(PrimarySuccessionPlanRecord item)
    {
        return new PrimarySuccessionPlanRecordView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Region,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimarySuccessionPlanRecordView> ToPrimarySuccessionPlanRecordViews(List<PrimarySuccessionPlanRecord> items)
    {
        var views = new List<PrimarySuccessionPlanRecordView>();
        foreach (var item in items)
        {
            views.Add(ToPrimarySuccessionPlanRecordView(item));
        }
        return views;
    }

    public static double SumPrimarySuccessionPlanRecordView(PrimarySuccessionPlanRecordView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedLeaveDays + parsedWeight;
    }
}
