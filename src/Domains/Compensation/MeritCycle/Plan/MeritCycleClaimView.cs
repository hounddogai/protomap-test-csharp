namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle claim.</summary>
public sealed record MeritCycleClaimView(string CodeText, string Title, string JobCode, string Source, string RatingText, string ContactEmailText, string MaximumText, string Score, string Active, string StatusLabel)
{
    public static MeritCycleClaimView ToMeritCycleClaimView(MeritCycleClaim item)
    {
        return new MeritCycleClaimView(
            Text.Truncate(item.Code, 120),
            item.Title,
            Text.Truncate(item.JobCode, 40),
            item.Source,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 120),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleClaimView> ToMeritCycleClaimViews(List<MeritCycleClaim> items)
    {
        var views = new List<MeritCycleClaimView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleClaimView(item));
        }
        return views;
    }

    public static double SumMeritCycleClaimView(MeritCycleClaimView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedRating + parsedMaximum + parsedScore;
    }
}
