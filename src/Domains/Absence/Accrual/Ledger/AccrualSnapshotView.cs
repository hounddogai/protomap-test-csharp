namespace Acme.Workforce.Absence.Accrual.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual snapshot.</summary>
public sealed record AccrualSnapshotView(string Code, string Title, string EndsAt, string DiscountText, string ContactEmailText, string StatusLabel)
{
    public static AccrualSnapshotView ToAccrualSnapshotView(AccrualSnapshot item)
    {
        return new AccrualSnapshotView(
            item.Code,
            item.Title,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualSnapshotView> ToAccrualSnapshotViews(List<AccrualSnapshot> items)
    {
        var views = new List<AccrualSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualSnapshotView(item));
        }
        return views;
    }

    public static double SumAccrualSnapshotView(AccrualSnapshotView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedDiscount;
    }
}
