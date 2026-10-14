namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

using System.Globalization;

/// <summary>How screens and exports show a pending warranty allocation.</summary>
public sealed record PendingWarrantyAllocationView(string Code, string TitleText, string JobCodeText, string Region, string EmployeeId, string Source, string Amount, string StatusLabel)
{
    public static PendingWarrantyAllocationView ToPendingWarrantyAllocationView(PendingWarrantyAllocation item)
    {
        return new PendingWarrantyAllocationView(
            item.Code,
            item.Title,
            item.JobCode,
            item.Region,
            item.EmployeeId,
            item.Source,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingWarrantyAllocationView> ToPendingWarrantyAllocationViews(List<PendingWarrantyAllocation> items)
    {
        var views = new List<PendingWarrantyAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToPendingWarrantyAllocationView(item));
        }
        return views;
    }

    public static double SumPendingWarrantyAllocationView(PendingWarrantyAllocationView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
