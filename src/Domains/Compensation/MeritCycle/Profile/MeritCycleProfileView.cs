namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle profile.</summary>
public sealed record MeritCycleProfileView(string Code, string TitleText, string Reviewer, string ExpiresAtText, string RateText, string Score, string Region, string ContactEmail, string StatusLabel)
{
    public static MeritCycleProfileView ToMeritCycleProfileView(MeritCycleProfile item)
    {
        return new MeritCycleProfileView(
            item.Code,
            item.Title,
            Text.Truncate(item.Reviewer, 120),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleProfileView> ToMeritCycleProfileViews(List<MeritCycleProfile> items)
    {
        var views = new List<MeritCycleProfileView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleProfileView(item));
        }
        return views;
    }

    public static double SumMeritCycleProfileView(MeritCycleProfileView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedRate + parsedScore;
    }
}
