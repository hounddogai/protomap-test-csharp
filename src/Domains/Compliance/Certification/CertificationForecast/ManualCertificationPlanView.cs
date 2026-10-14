namespace Acme.Workforce.Compliance.Certification.CertificationForecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual certification plan.</summary>
public sealed record ManualCertificationPlanView(string Code, string NameText, string LocationText, string Limit, string Locale, string Cost, string Segment, string Approved, string StartsAtText, string WindowDaysText, string Version, string StatusLabel)
{
    public static ManualCertificationPlanView ToManualCertificationPlanView(ManualCertificationPlan item)
    {
        return new ManualCertificationPlanView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Location,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Approved}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualCertificationPlanView> ToManualCertificationPlanViews(List<ManualCertificationPlan> items)
    {
        var views = new List<ManualCertificationPlanView>();
        foreach (var item in items)
        {
            views.Add(ToManualCertificationPlanView(item));
        }
        return views;
    }

    public static double SumManualCertificationPlanView(ManualCertificationPlanView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedCost + (double)parsedStartsAt;
    }
}
