namespace Acme.Workforce.Assets.LicenseSeat.SeatCycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy license seat cycle.</summary>
public sealed record LegacyLicenseSeatCycleView(string Code, string NameText, string RetriesText, string Cost, string PinnedText, string Limit, string Score, string Reviewer, string DescriptionText, string LeaveDays, string StatusLabel)
{
    public static LegacyLicenseSeatCycleView ToLegacyLicenseSeatCycleView(LegacyLicenseSeatCycle item)
    {
        return new LegacyLicenseSeatCycleView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            Text.Truncate(item.Description, 120),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyLicenseSeatCycleView> ToLegacyLicenseSeatCycleViews(List<LegacyLicenseSeatCycle> items)
    {
        var views = new List<LegacyLicenseSeatCycleView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyLicenseSeatCycleView(item));
        }
        return views;
    }

    public static double SumLegacyLicenseSeatCycleView(LegacyLicenseSeatCycleView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedCost + (double)parsedLimit;
    }
}
