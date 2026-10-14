namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global retro pay batch.</summary>
public sealed record GlobalRetroPayBatchView(string Code, string NameText, string ExternalRef, string Amount, string Grade, string Timezone, string Cost, string StatusLabel)
{
    public static GlobalRetroPayBatchView ToGlobalRetroPayBatchView(GlobalRetroPayBatch item)
    {
        return new GlobalRetroPayBatchView(
            item.Code,
            item.Name,
            item.ExternalRef,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 40),
            Text.Truncate(item.Timezone, 40),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalRetroPayBatchView> ToGlobalRetroPayBatchViews(List<GlobalRetroPayBatch> items)
    {
        var views = new List<GlobalRetroPayBatchView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalRetroPayBatchView(item));
        }
        return views;
    }

    public static double SumGlobalRetroPayBatchView(GlobalRetroPayBatchView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedAmount + parsedCost;
    }
}
