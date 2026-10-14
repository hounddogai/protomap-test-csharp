namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary merit cycle rollup.</summary>
public sealed record PrimaryMeritCycleRollupView(string Code, string TitleText, string Currency, string Archived, string Skill, string ExpiresAt, string GraceDaysText, string Summary, string Label, string StatusLabel)
{
    public static PrimaryMeritCycleRollupView ToPrimaryMeritCycleRollupView(PrimaryMeritCycleRollup item)
    {
        return new PrimaryMeritCycleRollupView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Currency,
            $"{item.Archived}",
            item.Skill,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryMeritCycleRollupView> ToPrimaryMeritCycleRollupViews(List<PrimaryMeritCycleRollup> items)
    {
        var views = new List<PrimaryMeritCycleRollupView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryMeritCycleRollupView(item));
        }
        return views;
    }

    public static double SumPrimaryMeritCycleRollupView(PrimaryMeritCycleRollupView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedGraceDays;
    }
}
