namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped adjustment agreement.</summary>
public sealed record CappedAdjustmentAgreementView(string Code, string NameText, string Retries, string Cost, string Quantity, string LocationText, string Budget, string StatusLabel)
{
    public static CappedAdjustmentAgreementView ToCappedAdjustmentAgreementView(CappedAdjustmentAgreement item)
    {
        return new CappedAdjustmentAgreementView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedAdjustmentAgreementView> ToCappedAdjustmentAgreementViews(List<CappedAdjustmentAgreement> items)
    {
        var views = new List<CappedAdjustmentAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToCappedAdjustmentAgreementView(item));
        }
        return views;
    }

    public static double SumCappedAdjustmentAgreementView(CappedAdjustmentAgreementView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedCost + (double)parsedQuantity;
    }
}
