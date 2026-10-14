namespace Acme.Workforce.Assets.AssetReturn.Segment;

using System.Globalization;

/// <summary>How screens and exports show a legacy asset return rule.</summary>
public sealed record LegacyAssetReturnRuleView(string Code, string Name, string Attempts, string NotesText, string SourceText, string GraceDays, string StatusLabel)
{
    public static LegacyAssetReturnRuleView ToLegacyAssetReturnRuleView(LegacyAssetReturnRule item)
    {
        return new LegacyAssetReturnRuleView(
            item.Code,
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Source,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyAssetReturnRuleView> ToLegacyAssetReturnRuleViews(List<LegacyAssetReturnRule> items)
    {
        var views = new List<LegacyAssetReturnRuleView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyAssetReturnRuleView(item));
        }
        return views;
    }

    public static double SumLegacyAssetReturnRuleView(LegacyAssetReturnRuleView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedGraceDays;
    }
}
