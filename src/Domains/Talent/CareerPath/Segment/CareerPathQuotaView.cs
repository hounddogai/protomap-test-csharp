namespace Acme.Workforce.Talent.CareerPath.Segment;

using System.Globalization;

/// <summary>How screens and exports show a career path quota.</summary>
public sealed record CareerPathQuotaView(string CodeText, string TitleText, string Score, string CostCenterText, string ReviewerText, string Locale, string StatusLabel)
{
    public static CareerPathQuotaView ToCareerPathQuotaView(CareerPathQuota item)
    {
        return new CareerPathQuotaView(
            item.Code,
            item.Title,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Reviewer,
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathQuotaView> ToCareerPathQuotaViews(List<CareerPathQuota> items)
    {
        var views = new List<CareerPathQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathQuotaView(item));
        }
        return views;
    }

    public static double SumCareerPathQuotaView(CareerPathQuotaView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
