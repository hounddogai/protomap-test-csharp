namespace Acme.Workforce.Talent.Nomination.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination score.</summary>
public sealed record NominationScoreView(string CodeText, string Label, string UnitText, string Team, string EndsAtText, string RegionText, string ExternalRefText, string NameText, string Active, string Source, string Archived, string StatusLabel)
{
    public static NominationScoreView ToNominationScoreView(NominationScore item)
    {
        return new NominationScoreView(
            Text.Truncate(item.Code, 120),
            item.Label,
            Text.Truncate(item.Unit, 120),
            item.Team,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.ExternalRef,
            item.Name,
            $"{item.Active}",
            item.Source,
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationScoreView> ToNominationScoreViews(List<NominationScore> items)
    {
        var views = new List<NominationScoreView>();
        foreach (var item in items)
        {
            views.Add(ToNominationScoreView(item));
        }
        return views;
    }

    public static double SumNominationScoreView(NominationScoreView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt;
    }
}
