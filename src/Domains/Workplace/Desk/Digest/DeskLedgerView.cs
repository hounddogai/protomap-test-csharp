namespace Acme.Workforce.Workplace.Desk.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk ledger.</summary>
public sealed record DeskLedgerView(string Code, string Title, string Discount, string Active, string OwnerId, string GradeText, string NotesText, string RatioText, string StatusLabel)
{
    public static DeskLedgerView ToDeskLedgerView(DeskLedger item)
    {
        return new DeskLedgerView(
            item.Code,
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.OwnerId,
            item.Grade,
            Text.Truncate(item.Notes, 80),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskLedgerView> ToDeskLedgerViews(List<DeskLedger> items)
    {
        var views = new List<DeskLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToDeskLedgerView(item));
        }
        return views;
    }

    public static double SumDeskLedgerView(DeskLedgerView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedRatio;
    }
}
