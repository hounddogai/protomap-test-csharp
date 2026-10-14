namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary headcount ledger.</summary>
public sealed record PrimaryHeadcountLedgerView(string CodeText, string TitleText, string ReviewerText, string Timezone, string Revision, string Threshold, string Discount, string QuantityText, string StatusLabel)
{
    public static PrimaryHeadcountLedgerView ToPrimaryHeadcountLedgerView(PrimaryHeadcountLedger item)
    {
        return new PrimaryHeadcountLedgerView(
            item.Code,
            item.Title,
            Text.Truncate(item.Reviewer, 120),
            Text.Truncate(item.Timezone, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryHeadcountLedgerView> ToPrimaryHeadcountLedgerViews(List<PrimaryHeadcountLedger> items)
    {
        var views = new List<PrimaryHeadcountLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryHeadcountLedgerView(item));
        }
        return views;
    }

    public static double SumPrimaryHeadcountLedgerView(PrimaryHeadcountLedgerView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedThreshold + parsedDiscount;
    }
}
