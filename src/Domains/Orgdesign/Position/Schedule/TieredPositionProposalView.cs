namespace Acme.Workforce.Orgdesign.Position.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered position proposal.</summary>
public sealed record TieredPositionProposalView(string Code, string Label, string StartsAt, string Margin, string WindowDays, string DepartmentText, string Skill, string Hours, string CostText, string StatusLabel)
{
    public static TieredPositionProposalView ToTieredPositionProposalView(TieredPositionProposal item)
    {
        return new TieredPositionProposalView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Skill,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPositionProposalView> ToTieredPositionProposalViews(List<TieredPositionProposal> items)
    {
        var views = new List<TieredPositionProposalView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPositionProposalView(item));
        }
        return views;
    }

    public static double SumTieredPositionProposalView(TieredPositionProposalView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedMargin + (double)parsedWindowDays;
    }
}
