namespace Acme.Workforce.Absence.Accrual.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending accrual window.</summary>
public sealed record PendingAccrualWindowView(string Code, string Title, string StartsAt, string SalaryBandText, string LocationText, string Category, string StatusLabel)
{
    public static PendingAccrualWindowView ToPendingAccrualWindowView(PendingAccrualWindow item)
    {
        return new PendingAccrualWindowView(
            item.Code,
            item.Title,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Location,
            Text.Truncate(item.Category, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingAccrualWindowView> ToPendingAccrualWindowViews(List<PendingAccrualWindow> items)
    {
        var views = new List<PendingAccrualWindowView>();
        foreach (var item in items)
        {
            views.Add(ToPendingAccrualWindowView(item));
        }
        return views;
    }

    public static double SumPendingAccrualWindowView(PendingAccrualWindowView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
