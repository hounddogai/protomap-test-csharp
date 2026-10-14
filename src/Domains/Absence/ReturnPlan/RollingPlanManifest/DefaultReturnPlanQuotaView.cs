namespace Acme.Workforce.Absence.ReturnPlan.RollingPlanManifest;

using System.Globalization;

/// <summary>How screens and exports show a default return plan quota.</summary>
public sealed record DefaultReturnPlanQuotaView(string Code, string Name, string RatingText, string WindowDays, string ThresholdText, string ExpiresAt, string Active, string StatusLabel)
{
    public static DefaultReturnPlanQuotaView ToDefaultReturnPlanQuotaView(DefaultReturnPlanQuota item)
    {
        return new DefaultReturnPlanQuotaView(
            item.Code,
            item.Name,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultReturnPlanQuotaView> ToDefaultReturnPlanQuotaViews(List<DefaultReturnPlanQuota> items)
    {
        var views = new List<DefaultReturnPlanQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultReturnPlanQuotaView(item));
        }
        return views;
    }

    public static double SumDefaultReturnPlanQuotaView(DefaultReturnPlanQuotaView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedWindowDays + parsedThreshold;
    }
}
