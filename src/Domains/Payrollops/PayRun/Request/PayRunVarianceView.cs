namespace Acme.Workforce.Payrollops.PayRun.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run variance.</summary>
public sealed record PayRunVarianceView(string Code, string Title, string CapacityText, string Quantity, string DurationMinutesText, string BudgetText, string StatusLabel)
{
    public static PayRunVarianceView ToPayRunVarianceView(PayRunVariance item)
    {
        return new PayRunVarianceView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunVarianceView> ToPayRunVarianceViews(List<PayRunVariance> items)
    {
        var views = new List<PayRunVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunVarianceView(item));
        }
        return views;
    }

    public static double SumPayRunVarianceView(PayRunVarianceView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedQuantity + (double)parsedDurationMinutes;
    }
}
