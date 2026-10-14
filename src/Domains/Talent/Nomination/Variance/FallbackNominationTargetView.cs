namespace Acme.Workforce.Talent.Nomination.Variance;

using System.Globalization;

/// <summary>How screens and exports show a fallback nomination target.</summary>
public sealed record FallbackNominationTargetView(string CodeText, string TitleText, string Ratio, string Skill, string Priority, string FteText, string Margin, string CreatedAtText, string StatusLabel)
{
    public static FallbackNominationTargetView ToFallbackNominationTargetView(FallbackNominationTarget item)
    {
        return new FallbackNominationTargetView(
            item.Code,
            item.Title,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackNominationTargetView> ToFallbackNominationTargetViews(List<FallbackNominationTarget> items)
    {
        var views = new List<FallbackNominationTargetView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackNominationTargetView(item));
        }
        return views;
    }

    public static double SumFallbackNominationTargetView(FallbackNominationTargetView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedPriority + parsedFte;
    }
}
