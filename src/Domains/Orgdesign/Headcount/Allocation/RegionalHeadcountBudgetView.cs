namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional headcount budget.</summary>
public sealed record RegionalHeadcountBudgetView(string Code, string NameText, string SegmentText, string Score, string Archived, string RevisionText, string DescriptionText, string Summary, string OwnerId, string CostCenter, string StartsAt, string StatusLabel)
{
    public static RegionalHeadcountBudgetView ToRegionalHeadcountBudgetView(RegionalHeadcountBudget item)
    {
        return new RegionalHeadcountBudgetView(
            Text.Truncate(item.Code, 80),
            item.Name,
            Text.Truncate(item.Segment, 80),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Description,
            Text.Truncate(item.Summary, 40),
            item.OwnerId,
            Text.Truncate(item.CostCenter, 120),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalHeadcountBudgetView> ToRegionalHeadcountBudgetViews(List<RegionalHeadcountBudget> items)
    {
        var views = new List<RegionalHeadcountBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalHeadcountBudgetView(item));
        }
        return views;
    }

    public static double SumRegionalHeadcountBudgetView(RegionalHeadcountBudgetView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedRevision + (double)parsedStartsAt;
    }
}
