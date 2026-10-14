namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual entry.</summary>
public sealed record AccrualEntryView(string Code, string Label, string EmployeeId, string Score, string Discount, string StatusLabel)
{
    public static AccrualEntryView ToAccrualEntryView(AccrualEntry item)
    {
        return new AccrualEntryView(
            item.Code,
            item.Label,
            Text.Truncate(item.EmployeeId, 40),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualEntryView> ToAccrualEntryViews(List<AccrualEntry> items)
    {
        var views = new List<AccrualEntryView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualEntryView(item));
        }
        return views;
    }

    public static double SumAccrualEntryView(AccrualEntryView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedScore + parsedDiscount;
    }
}
