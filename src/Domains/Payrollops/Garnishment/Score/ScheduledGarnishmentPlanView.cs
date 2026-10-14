namespace Acme.Workforce.Payrollops.Garnishment.Score;

using System.Globalization;

/// <summary>How screens and exports show a scheduled garnishment plan.</summary>
public sealed record ScheduledGarnishmentPlanView(string Code, string Title, string WeightText, string SalaryBandText, string VersionText, string Channel, string StatusLabel)
{
    public static ScheduledGarnishmentPlanView ToScheduledGarnishmentPlanView(ScheduledGarnishmentPlan item)
    {
        return new ScheduledGarnishmentPlanView(
            item.Code,
            item.Title,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledGarnishmentPlanView> ToScheduledGarnishmentPlanViews(List<ScheduledGarnishmentPlan> items)
    {
        var views = new List<ScheduledGarnishmentPlanView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledGarnishmentPlanView(item));
        }
        return views;
    }

    public static double SumScheduledGarnishmentPlanView(ScheduledGarnishmentPlanView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedVersion;
    }
}
