namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using System.Globalization;

/// <summary>How screens and exports show a pending tax form override.</summary>
public sealed record PendingTaxFormOverrideView(string Code, string TitleText, string Archived, string Amount, string FloorValueText, string BudgetText, string StatusLabel)
{
    public static PendingTaxFormOverrideView ToPendingTaxFormOverrideView(PendingTaxFormOverride item)
    {
        return new PendingTaxFormOverrideView(
            item.Code,
            item.Title,
            $"{item.Archived}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingTaxFormOverrideView> ToPendingTaxFormOverrideViews(List<PendingTaxFormOverride> items)
    {
        var views = new List<PendingTaxFormOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToPendingTaxFormOverrideView(item));
        }
        return views;
    }

    public static double SumPendingTaxFormOverrideView(PendingTaxFormOverrideView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedAmount + parsedFloorValue + parsedBudget;
    }
}
