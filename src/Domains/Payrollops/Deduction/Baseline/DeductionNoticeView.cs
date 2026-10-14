namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using System.Globalization;

/// <summary>How screens and exports show a deduction notice.</summary>
public sealed record DeductionNoticeView(string Code, string Label, string TitleText, string CostCenter, string Fte, string QuantityText, string Visible, string Cost, string StatusLabel)
{
    public static DeductionNoticeView ToDeductionNoticeView(DeductionNotice item)
    {
        return new DeductionNoticeView(
            item.Code,
            item.Label,
            item.Title,
            item.CostCenter,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionNoticeView> ToDeductionNoticeViews(List<DeductionNotice> items)
    {
        var views = new List<DeductionNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionNoticeView(item));
        }
        return views;
    }

    public static double SumDeductionNoticeView(DeductionNoticeView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedQuantity + parsedCost;
    }
}
