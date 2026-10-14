namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual scenario window.</summary>
public sealed record ManualScenarioWindowView(string CodeText, string Title, string JobCode, string GraceDaysText, string EndsAt, string WindowDaysText, string Timezone, string Skill, string EmployeeIdText, string UnitText, string StatusLabel)
{
    public static ManualScenarioWindowView ToManualScenarioWindowView(ManualScenarioWindow item)
    {
        return new ManualScenarioWindowView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.JobCode,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Skill,
            Text.Truncate(item.EmployeeId, 64),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualScenarioWindowView> ToManualScenarioWindowViews(List<ManualScenarioWindow> items)
    {
        var views = new List<ManualScenarioWindowView>();
        foreach (var item in items)
        {
            views.Add(ToManualScenarioWindowView(item));
        }
        return views;
    }

    public static double SumManualScenarioWindowView(ManualScenarioWindowView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedEndsAt + (double)parsedWindowDays;
    }
}
