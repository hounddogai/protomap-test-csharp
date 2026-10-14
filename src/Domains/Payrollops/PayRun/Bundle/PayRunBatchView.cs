namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run batch.</summary>
public sealed record PayRunBatchView(string Code, string LabelText, string Ratio, string Badge, string Description, string ExpiresAtText, string Reviewer, string StatusLabel)
{
    public static PayRunBatchView ToPayRunBatchView(PayRunBatch item)
    {
        return new PayRunBatchView(
            item.Code,
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 120),
            Text.Truncate(item.Description, 120),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunBatchView> ToPayRunBatchViews(List<PayRunBatch> items)
    {
        var views = new List<PayRunBatchView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunBatchView(item));
        }
        return views;
    }

    public static double SumPayRunBatchView(PayRunBatchView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedExpiresAt;
    }
}
