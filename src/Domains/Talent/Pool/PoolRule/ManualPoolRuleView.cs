namespace Acme.Workforce.Talent.Pool.PoolRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual pool rule.</summary>
public sealed record ManualPoolRuleView(string CodeText, string NameText, string Fte, string QuantityText, string Limit, string GradeText, string Weight, string TitleText, string LabelText, string AmountText, string DurationMinutes, string StatusLabel)
{
    public static ManualPoolRuleView ToManualPoolRuleView(ManualPoolRule item)
    {
        return new ManualPoolRuleView(
            item.Code,
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            Text.Truncate(item.Label, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualPoolRuleView> ToManualPoolRuleViews(List<ManualPoolRule> items)
    {
        var views = new List<ManualPoolRuleView>();
        foreach (var item in items)
        {
            views.Add(ToManualPoolRuleView(item));
        }
        return views;
    }

    public static double SumManualPoolRuleView(ManualPoolRuleView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedQuantity + (double)parsedLimit;
    }
}
