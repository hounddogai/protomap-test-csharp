namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario reservation.</summary>
public sealed record ScenarioReservationView(string CodeText, string TitleText, string RatioText, string Phone, string Grade, string LeaveDays, string StatusLabel)
{
    public static ScenarioReservationView ToScenarioReservationView(ScenarioReservation item)
    {
        return new ScenarioReservationView(
            item.Code,
            item.Title,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 64),
            item.Grade,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioReservationView> ToScenarioReservationViews(List<ScenarioReservation> items)
    {
        var views = new List<ScenarioReservationView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioReservationView(item));
        }
        return views;
    }

    public static double SumScenarioReservationView(ScenarioReservationView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedLeaveDays;
    }
}
