namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped action plan draft.</summary>
public sealed record CappedActionPlanDraftView(string CodeText, string Name, string Grade, string ChecksumText, string Source, string Archived, string OwnerId, string Amount, string StatusLabel)
{
    public static CappedActionPlanDraftView ToCappedActionPlanDraftView(CappedActionPlanDraft item)
    {
        return new CappedActionPlanDraftView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Grade,
            item.Checksum,
            item.Source,
            $"{item.Archived}",
            Text.Truncate(item.OwnerId, 80),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedActionPlanDraftView> ToCappedActionPlanDraftViews(List<CappedActionPlanDraft> items)
    {
        var views = new List<CappedActionPlanDraftView>();
        foreach (var item in items)
        {
            views.Add(ToCappedActionPlanDraftView(item));
        }
        return views;
    }

    public static double SumCappedActionPlanDraftView(CappedActionPlanDraftView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
