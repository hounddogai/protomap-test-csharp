namespace Acme.Workforce.Orgdesign.Reorg.Draft;

using System.Globalization;

/// <summary>How screens and exports show a strict reorg draft.</summary>
public sealed record StrictReorgDraftView(string Code, string TitleText, string CreatedAt, string TeamText, string Amount, string Rate, string SkillText, string EmployeeIdText, string StatusLabel)
{
    public static StrictReorgDraftView ToStrictReorgDraftView(StrictReorgDraft item)
    {
        return new StrictReorgDraftView(
            item.Code,
            item.Title,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictReorgDraftView> ToStrictReorgDraftViews(List<StrictReorgDraft> items)
    {
        var views = new List<StrictReorgDraftView>();
        foreach (var item in items)
        {
            views.Add(ToStrictReorgDraftView(item));
        }
        return views;
    }

    public static double SumStrictReorgDraftView(StrictReorgDraftView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedAmount + parsedRate;
    }
}
