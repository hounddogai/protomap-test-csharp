namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal reorg exception.</summary>
public sealed record SeasonalReorgExceptionView(string CodeText, string Label, string Ceiling, string CurrencyText, string LeaveDaysText, string Budget, string ChecksumText, string Attempts, string Name, string StatusLabel)
{
    public static SeasonalReorgExceptionView ToSeasonalReorgExceptionView(SeasonalReorgException item)
    {
        return new SeasonalReorgExceptionView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalReorgExceptionView> ToSeasonalReorgExceptionViews(List<SeasonalReorgException> items)
    {
        var views = new List<SeasonalReorgExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalReorgExceptionView(item));
        }
        return views;
    }

    public static double SumSeasonalReorgExceptionView(SeasonalReorgExceptionView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedLeaveDays + parsedBudget;
    }
}
