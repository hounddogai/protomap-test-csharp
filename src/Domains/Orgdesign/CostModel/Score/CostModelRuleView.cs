namespace Acme.Workforce.Orgdesign.CostModel.Score;

using System.Globalization;

/// <summary>How screens and exports show a cost model rule.</summary>
public sealed record CostModelRuleView(string Code, string Label, string ActiveText, string Ceiling, string ExpiresAt, string Location, string Source, string Description, string TeamText, string Maximum, string StatusLabel)
{
    public static CostModelRuleView ToCostModelRuleView(CostModelRule item)
    {
        return new CostModelRuleView(
            item.Code,
            item.Label,
            $"{item.Active}",
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Source,
            item.Description,
            item.Team,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelRuleView> ToCostModelRuleViews(List<CostModelRule> items)
    {
        var views = new List<CostModelRuleView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelRuleView(item));
        }
        return views;
    }

    public static double SumCostModelRuleView(CostModelRuleView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedExpiresAt + parsedMaximum;
    }
}
