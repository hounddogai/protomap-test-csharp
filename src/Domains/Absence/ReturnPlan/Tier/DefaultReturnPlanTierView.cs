namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using System.Globalization;

/// <summary>How screens and exports show a default return plan tier.</summary>
public sealed record DefaultReturnPlanTierView(string Code, string Label, string DurationMinutes, string WindowDays, string Channel, string PinnedText, string CreatedAt, string StatusLabel)
{
    public static DefaultReturnPlanTierView ToDefaultReturnPlanTierView(DefaultReturnPlanTier item)
    {
        return new DefaultReturnPlanTierView(
            item.Code,
            item.Label,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Pinned}",
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultReturnPlanTierView> ToDefaultReturnPlanTierViews(List<DefaultReturnPlanTier> items)
    {
        var views = new List<DefaultReturnPlanTierView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultReturnPlanTierView(item));
        }
        return views;
    }

    public static double SumDefaultReturnPlanTierView(DefaultReturnPlanTierView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedWindowDays + (double)parsedCreatedAt;
    }
}
