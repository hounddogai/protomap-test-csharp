namespace Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient succession plan score.</summary>
public sealed record LenientSuccessionPlanScoreView(string Code, string LabelText, string EndsAtText, string Country, string Limit, string RevisionText, string StatusLabel)
{
    public static LenientSuccessionPlanScoreView ToLenientSuccessionPlanScoreView(LenientSuccessionPlanScore item)
    {
        return new LenientSuccessionPlanScoreView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 80),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientSuccessionPlanScoreView> ToLenientSuccessionPlanScoreViews(List<LenientSuccessionPlanScore> items)
    {
        var views = new List<LenientSuccessionPlanScoreView>();
        foreach (var item in items)
        {
            views.Add(ToLenientSuccessionPlanScoreView(item));
        }
        return views;
    }

    public static double SumLenientSuccessionPlanScoreView(LenientSuccessionPlanScoreView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedLimit + (double)parsedRevision;
    }
}
