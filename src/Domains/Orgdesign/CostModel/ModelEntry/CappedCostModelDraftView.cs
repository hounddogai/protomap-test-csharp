namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped cost model draft.</summary>
public sealed record CappedCostModelDraftView(string Code, string TitleText, string Reviewer, string MarginText, string Skill, string TenureMonths, string Ratio, string Badge, string Department, string StatusLabel)
{
    public static CappedCostModelDraftView ToCappedCostModelDraftView(CappedCostModelDraft item)
    {
        return new CappedCostModelDraftView(
            item.Code,
            item.Title,
            item.Reviewer,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 64),
            Text.Truncate(item.Department, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedCostModelDraftView> ToCappedCostModelDraftViews(List<CappedCostModelDraft> items)
    {
        var views = new List<CappedCostModelDraftView>();
        foreach (var item in items)
        {
            views.Add(ToCappedCostModelDraftView(item));
        }
        return views;
    }

    public static double SumCappedCostModelDraftView(CappedCostModelDraftView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedTenureMonths + parsedRatio;
    }
}
