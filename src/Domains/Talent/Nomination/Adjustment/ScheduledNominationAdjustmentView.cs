namespace Acme.Workforce.Talent.Nomination.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled nomination adjustment.</summary>
public sealed record ScheduledNominationAdjustmentView(string Code, string LabelText, string FloorValueText, string ExternalRefText, string Hours, string StatusLabel)
{
    public static ScheduledNominationAdjustmentView ToScheduledNominationAdjustmentView(ScheduledNominationAdjustment item)
    {
        return new ScheduledNominationAdjustmentView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledNominationAdjustmentView> ToScheduledNominationAdjustmentViews(List<ScheduledNominationAdjustment> items)
    {
        var views = new List<ScheduledNominationAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledNominationAdjustmentView(item));
        }
        return views;
    }

    public static double SumScheduledNominationAdjustmentView(ScheduledNominationAdjustmentView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedHours;
    }
}
