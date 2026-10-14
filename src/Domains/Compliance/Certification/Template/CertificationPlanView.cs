namespace Acme.Workforce.Compliance.Certification.Template;

using System.Globalization;

/// <summary>How screens and exports show a certification plan.</summary>
public sealed record CertificationPlanView(string Code, string NameText, string VersionText, string FloorValue, string Score, string Grade, string Badge, string Channel, string DurationMinutesText, string StatusLabel)
{
    public static CertificationPlanView ToCertificationPlanView(CertificationPlan item)
    {
        return new CertificationPlanView(
            item.Code,
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Badge,
            item.Channel,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationPlanView> ToCertificationPlanViews(List<CertificationPlan> items)
    {
        var views = new List<CertificationPlanView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationPlanView(item));
        }
        return views;
    }

    public static double SumCertificationPlanView(CertificationPlanView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedFloorValue + parsedScore;
    }
}
