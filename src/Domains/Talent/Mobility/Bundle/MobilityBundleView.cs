namespace Acme.Workforce.Talent.Mobility.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility bundle.</summary>
public sealed record MobilityBundleView(string Code, string TitleText, string FloorValue, string RetriesText, string BudgetText, string EndsAtText, string Fte, string Priority, string Grade, string TenureMonths, string StatusLabel)
{
    public static MobilityBundleView ToMobilityBundleView(MobilityBundle item)
    {
        return new MobilityBundleView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 64),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityBundleView> ToMobilityBundleViews(List<MobilityBundle> items)
    {
        var views = new List<MobilityBundleView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityBundleView(item));
        }
        return views;
    }

    public static double SumMobilityBundleView(MobilityBundleView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedRetries + parsedBudget;
    }
}
