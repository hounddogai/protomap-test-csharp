namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg draft.</summary>
public sealed record ReorgDraftView(string Code, string Name, string Weight, string Limit, string RegionText, string Label, string SkillText, string StatusLabel)
{
    public static ReorgDraftView ToReorgDraftView(ReorgDraft item)
    {
        return new ReorgDraftView(
            item.Code,
            item.Name,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Region,
            Text.Truncate(item.Label, 120),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgDraftView> ToReorgDraftViews(List<ReorgDraft> items)
    {
        var views = new List<ReorgDraftView>();
        foreach (var item in items)
        {
            views.Add(ToReorgDraftView(item));
        }
        return views;
    }

    public static double SumReorgDraftView(ReorgDraftView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedLimit;
    }
}
