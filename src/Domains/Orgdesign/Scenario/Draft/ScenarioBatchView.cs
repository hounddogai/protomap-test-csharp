namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario batch.</summary>
public sealed record ScenarioBatchView(string Code, string Label, string RateText, string Visible, string Title, string MaximumText, string Attempts, string Active, string Margin, string Limit, string SalaryBandText, string StatusLabel)
{
    public static ScenarioBatchView ToScenarioBatchView(ScenarioBatch item)
    {
        return new ScenarioBatchView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioBatchView> ToScenarioBatchViews(List<ScenarioBatch> items)
    {
        var views = new List<ScenarioBatchView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioBatchView(item));
        }
        return views;
    }

    public static double SumScenarioBatchView(ScenarioBatchView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedRate + parsedMaximum + (double)parsedAttempts;
    }
}
