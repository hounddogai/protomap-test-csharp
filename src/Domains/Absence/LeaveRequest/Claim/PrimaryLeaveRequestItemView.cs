namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using System.Globalization;

/// <summary>How screens and exports show a primary leave request item.</summary>
public sealed record PrimaryLeaveRequestItemView(string Code, string Name, string RetriesText, string AmountText, string DiscountText, string Attempts, string Active, string Weight, string ExternalRefText, string Description, string Cost, string StatusLabel)
{
    public static PrimaryLeaveRequestItemView ToPrimaryLeaveRequestItemView(PrimaryLeaveRequestItem item)
    {
        return new PrimaryLeaveRequestItemView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Description,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryLeaveRequestItemView> ToPrimaryLeaveRequestItemViews(List<PrimaryLeaveRequestItem> items)
    {
        var views = new List<PrimaryLeaveRequestItemView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryLeaveRequestItemView(item));
        }
        return views;
    }

    public static double SumPrimaryLeaveRequestItemView(PrimaryLeaveRequestItemView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedAmount + parsedDiscount;
    }
}
