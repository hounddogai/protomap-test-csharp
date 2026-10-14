namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered scenario window.</summary>
public sealed record TieredScenarioWindowView(string Code, string LabelText, string WindowDays, string Maximum, string Unit, string RatioText, string StatusLabel)
{
    public static TieredScenarioWindowView ToTieredScenarioWindowView(TieredScenarioWindow item)
    {
        return new TieredScenarioWindowView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredScenarioWindowView> ToTieredScenarioWindowViews(List<TieredScenarioWindow> items)
    {
        var views = new List<TieredScenarioWindowView>();
        foreach (var item in items)
        {
            views.Add(ToTieredScenarioWindowView(item));
        }
        return views;
    }

    public static double SumTieredScenarioWindowView(TieredScenarioWindowView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedMaximum + parsedRatio;
    }
}
