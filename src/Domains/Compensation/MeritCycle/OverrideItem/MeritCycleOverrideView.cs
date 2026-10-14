namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using System.Globalization;

/// <summary>How screens and exports show a merit cycle override.</summary>
public sealed record MeritCycleOverrideView(string CodeText, string Label, string Category, string Locale, string OwnerId, string AmountText, string StatusLabel)
{
    public static MeritCycleOverrideView ToMeritCycleOverrideView(MeritCycleOverride item)
    {
        return new MeritCycleOverrideView(
            item.Code,
            item.Label,
            item.Category,
            item.Locale,
            item.OwnerId,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleOverrideView> ToMeritCycleOverrideViews(List<MeritCycleOverride> items)
    {
        var views = new List<MeritCycleOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleOverrideView(item));
        }
        return views;
    }

    public static double SumMeritCycleOverrideView(MeritCycleOverrideView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
