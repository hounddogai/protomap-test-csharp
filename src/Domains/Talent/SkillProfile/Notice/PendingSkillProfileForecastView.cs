namespace Acme.Workforce.Talent.SkillProfile.Notice;

using System.Globalization;

/// <summary>How screens and exports show a pending skill profile forecast.</summary>
public sealed record PendingSkillProfileForecastView(string Code, string Title, string Summary, string Attempts, string StatusLabel)
{
    public static PendingSkillProfileForecastView ToPendingSkillProfileForecastView(PendingSkillProfileForecast item)
    {
        return new PendingSkillProfileForecastView(
            item.Code,
            item.Title,
            item.Summary,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingSkillProfileForecastView> ToPendingSkillProfileForecastViews(List<PendingSkillProfileForecast> items)
    {
        var views = new List<PendingSkillProfileForecastView>();
        foreach (var item in items)
        {
            views.Add(ToPendingSkillProfileForecastView(item));
        }
        return views;
    }

    public static double SumPendingSkillProfileForecastView(PendingSkillProfileForecastView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedAttempts;
    }
}
