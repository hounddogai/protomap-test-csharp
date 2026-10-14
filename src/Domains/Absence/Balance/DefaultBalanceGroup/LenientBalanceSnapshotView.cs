namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient balance snapshot.</summary>
public sealed record LenientBalanceSnapshotView(string CodeText, string Title, string TenureMonthsText, string Source, string Name, string StartsAtText, string ExternalRef, string LabelText, string StatusLabel)
{
    public static LenientBalanceSnapshotView ToLenientBalanceSnapshotView(LenientBalanceSnapshot item)
    {
        return new LenientBalanceSnapshotView(
            item.Code,
            item.Title,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 40),
            Text.Truncate(item.Name, 80),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientBalanceSnapshotView> ToLenientBalanceSnapshotViews(List<LenientBalanceSnapshot> items)
    {
        var views = new List<LenientBalanceSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToLenientBalanceSnapshotView(item));
        }
        return views;
    }

    public static double SumLenientBalanceSnapshotView(LenientBalanceSnapshotView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedStartsAt;
    }
}
