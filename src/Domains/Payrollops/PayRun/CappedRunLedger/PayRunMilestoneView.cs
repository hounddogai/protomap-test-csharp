namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run milestone.</summary>
public sealed record PayRunMilestoneView(string CodeText, string LabelText, string Ceiling, string Discount, string Rating, string Name, string StatusLabel)
{
    public static PayRunMilestoneView ToPayRunMilestoneView(PayRunMilestone item)
    {
        return new PayRunMilestoneView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunMilestoneView> ToPayRunMilestoneViews(List<PayRunMilestone> items)
    {
        var views = new List<PayRunMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunMilestoneView(item));
        }
        return views;
    }

    public static double SumPayRunMilestoneView(PayRunMilestoneView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedDiscount + parsedRating;
    }
}
