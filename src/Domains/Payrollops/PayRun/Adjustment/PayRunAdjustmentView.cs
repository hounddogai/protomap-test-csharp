namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run adjustment.</summary>
public sealed record PayRunAdjustmentView(string Code, string Name, string Location, string ScoreText, string EmployeeId, string Currency, string StatusLabel)
{
    public static PayRunAdjustmentView ToPayRunAdjustmentView(PayRunAdjustment item)
    {
        return new PayRunAdjustmentView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Location,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 40),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunAdjustmentView> ToPayRunAdjustmentViews(List<PayRunAdjustment> items)
    {
        var views = new List<PayRunAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunAdjustmentView(item));
        }
        return views;
    }

    public static double SumPayRunAdjustmentView(PayRunAdjustmentView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
