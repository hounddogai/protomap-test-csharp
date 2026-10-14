namespace Acme.Workforce.Compensation.BonusPool.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool record.</summary>
public sealed record BonusPoolRecordView(string Code, string Label, string StartsAt, string Amount, string EndsAt, string Notes, string SalaryBand, string Attempts, string StatusLabel)
{
    public static BonusPoolRecordView ToBonusPoolRecordView(BonusPoolRecord item)
    {
        return new BonusPoolRecordView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 80),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            Text.Truncate(item.SalaryBand, 80),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolRecordView> ToBonusPoolRecordViews(List<BonusPoolRecord> items)
    {
        var views = new List<BonusPoolRecordView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolRecordView(item));
        }
        return views;
    }

    public static double SumBonusPoolRecordView(BonusPoolRecordView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedAmount + (double)parsedEndsAt;
    }
}
