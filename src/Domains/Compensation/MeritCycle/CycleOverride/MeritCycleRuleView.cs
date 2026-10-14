namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using System.Globalization;

/// <summary>How screens and exports show a merit cycle rule.</summary>
public sealed record MeritCycleRuleView(string Code, string Title, string Notes, string Approved, string Name, string Badge, string Amount, string StatusLabel)
{
    public static MeritCycleRuleView ToMeritCycleRuleView(MeritCycleRule item)
    {
        return new MeritCycleRuleView(
            item.Code,
            item.Title,
            item.Notes,
            $"{item.Approved}",
            item.Name,
            item.Badge,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleRuleView> ToMeritCycleRuleViews(List<MeritCycleRule> items)
    {
        var views = new List<MeritCycleRuleView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleRuleView(item));
        }
        return views;
    }

    public static double SumMeritCycleRuleView(MeritCycleRuleView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
