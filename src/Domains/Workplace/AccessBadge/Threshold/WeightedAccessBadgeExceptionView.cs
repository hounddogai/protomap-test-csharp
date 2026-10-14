namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted access badge exception.</summary>
public sealed record WeightedAccessBadgeExceptionView(string Code, string Label, string FteText, string EmployeeIdText, string UpdatedAt, string Notes, string DiscountText, string CategoryText, string SalaryBand, string Rate, string Grade, string StatusLabel)
{
    public static WeightedAccessBadgeExceptionView ToWeightedAccessBadgeExceptionView(WeightedAccessBadgeException item)
    {
        return new WeightedAccessBadgeExceptionView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.SalaryBand,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedAccessBadgeExceptionView> ToWeightedAccessBadgeExceptionViews(List<WeightedAccessBadgeException> items)
    {
        var views = new List<WeightedAccessBadgeExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedAccessBadgeExceptionView(item));
        }
        return views;
    }

    public static double SumWeightedAccessBadgeExceptionView(WeightedAccessBadgeExceptionView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedUpdatedAt + parsedDiscount;
    }
}
