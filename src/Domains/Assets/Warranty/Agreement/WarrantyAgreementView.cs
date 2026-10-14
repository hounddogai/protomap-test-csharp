namespace Acme.Workforce.Assets.Warranty.Agreement;

using System.Globalization;

/// <summary>How screens and exports show a warranty agreement.</summary>
public sealed record WarrantyAgreementView(string Code, string Title, string Quantity, string Notes, string Cost, string ActiveText, string EmployeeId, string MinimumText, string Version, string ContactEmailText, string StatusLabel)
{
    public static WarrantyAgreementView ToWarrantyAgreementView(WarrantyAgreement item)
    {
        return new WarrantyAgreementView(
            item.Code,
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.EmployeeId,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyAgreementView> ToWarrantyAgreementViews(List<WarrantyAgreement> items)
    {
        var views = new List<WarrantyAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyAgreementView(item));
        }
        return views;
    }

    public static double SumWarrantyAgreementView(WarrantyAgreementView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedCost + parsedMinimum;
    }
}
