namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending scenario request.</summary>
public sealed record PendingScenarioRequestView(string Code, string LabelText, string WindowDaysText, string Description, string Threshold, string Sequence, string Phone, string StatusLabel)
{
    public static PendingScenarioRequestView ToPendingScenarioRequestView(PendingScenarioRequest item)
    {
        return new PendingScenarioRequestView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingScenarioRequestView> ToPendingScenarioRequestViews(List<PendingScenarioRequest> items)
    {
        var views = new List<PendingScenarioRequestView>();
        foreach (var item in items)
        {
            views.Add(ToPendingScenarioRequestView(item));
        }
        return views;
    }

    public static double SumPendingScenarioRequestView(PendingScenarioRequestView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedThreshold + (double)parsedSequence;
    }
}
