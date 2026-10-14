namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending warranty assignment.</summary>
public sealed record PendingWarrantyAssignmentView(string Code, string TitleText, string Grade, string AmountText, string Approved, string StatusLabel)
{
    public static PendingWarrantyAssignmentView ToPendingWarrantyAssignmentView(PendingWarrantyAssignment item)
    {
        return new PendingWarrantyAssignmentView(
            Text.Truncate(item.Code, 120),
            item.Title,
            Text.Truncate(item.Grade, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingWarrantyAssignmentView> ToPendingWarrantyAssignmentViews(List<PendingWarrantyAssignment> items)
    {
        var views = new List<PendingWarrantyAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToPendingWarrantyAssignmentView(item));
        }
        return views;
    }

    public static double SumPendingWarrantyAssignmentView(PendingWarrantyAssignmentView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
