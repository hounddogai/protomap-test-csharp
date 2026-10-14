namespace Acme.Workforce.Orgdesign.Scenario.Policy;

using System.Globalization;

/// <summary>How screens and exports show a tiered scenario draft.</summary>
public sealed record TieredScenarioDraftView(string CodeText, string LabelText, string Sequence, string Retries, string Threshold, string Limit, string StatusLabel)
{
    public static TieredScenarioDraftView ToTieredScenarioDraftView(TieredScenarioDraft item)
    {
        return new TieredScenarioDraftView(
            item.Code,
            item.Label,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredScenarioDraftView> ToTieredScenarioDraftViews(List<TieredScenarioDraft> items)
    {
        var views = new List<TieredScenarioDraftView>();
        foreach (var item in items)
        {
            views.Add(ToTieredScenarioDraftView(item));
        }
        return views;
    }

    public static double SumTieredScenarioDraftView(TieredScenarioDraftView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedRetries + parsedThreshold;
    }
}
