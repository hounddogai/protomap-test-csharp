namespace Acme.Workforce.Assets.LicenseSeat.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy license seat window.</summary>
public sealed record LegacyLicenseSeatWindowView(string Code, string TitleText, string CeilingText, string JobCode, string GraceDays, string PriorityText, string Active, string Threshold, string Name, string StatusLabel)
{
    public static LegacyLicenseSeatWindowView ToLegacyLicenseSeatWindowView(LegacyLicenseSeatWindow item)
    {
        return new LegacyLicenseSeatWindowView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 64),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyLicenseSeatWindowView> ToLegacyLicenseSeatWindowViews(List<LegacyLicenseSeatWindow> items)
    {
        var views = new List<LegacyLicenseSeatWindowView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyLicenseSeatWindowView(item));
        }
        return views;
    }

    public static double SumLegacyLicenseSeatWindowView(LegacyLicenseSeatWindowView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedGraceDays + (double)parsedPriority;
    }
}
