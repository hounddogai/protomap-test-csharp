namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global access badge rollup.</summary>
public sealed record GlobalAccessBadgeRollupView(string Code, string Name, string Active, string Sequence, string Minimum, string GraceDays, string Cost, string Ceiling, string StatusLabel)
{
    public static GlobalAccessBadgeRollupView ToGlobalAccessBadgeRollupView(GlobalAccessBadgeRollup item)
    {
        return new GlobalAccessBadgeRollupView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 80),
            $"{item.Active}",
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalAccessBadgeRollupView> ToGlobalAccessBadgeRollupViews(List<GlobalAccessBadgeRollup> items)
    {
        var views = new List<GlobalAccessBadgeRollupView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalAccessBadgeRollupView(item));
        }
        return views;
    }

    public static double SumGlobalAccessBadgeRollupView(GlobalAccessBadgeRollupView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedMinimum + (double)parsedGraceDays;
    }
}
