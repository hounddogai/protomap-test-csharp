namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool manifest.</summary>
public sealed record BonusPoolManifestView(string Code, string TitleText, string Maximum, string JobCode, string Ceiling, string TenureMonthsText, string Source, string Retries, string Region, string DurationMinutes, string Attempts, string StatusLabel)
{
    public static BonusPoolManifestView ToBonusPoolManifestView(BonusPoolManifest item)
    {
        return new BonusPoolManifestView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 40),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 80),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolManifestView> ToBonusPoolManifestViews(List<BonusPoolManifest> items)
    {
        var views = new List<BonusPoolManifestView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolManifestView(item));
        }
        return views;
    }

    public static double SumBonusPoolManifestView(BonusPoolManifestView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedCeiling + (double)parsedTenureMonths;
    }
}
