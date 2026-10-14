namespace Acme.Workforce.Payrollops.PayRun.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run checkpoint.</summary>
public sealed record PayRunCheckpointView(string Code, string Label, string Team, string FloorValue, string PhoneText, string Margin, string BatchSize, string Weight, string DepartmentText, string GraceDays, string StatusLabel)
{
    public static PayRunCheckpointView ToPayRunCheckpointView(PayRunCheckpoint item)
    {
        return new PayRunCheckpointView(
            item.Code,
            item.Label,
            Text.Truncate(item.Team, 64),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 64),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 80),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunCheckpointView> ToPayRunCheckpointViews(List<PayRunCheckpoint> items)
    {
        var views = new List<PayRunCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunCheckpointView(item));
        }
        return views;
    }

    public static double SumPayRunCheckpointView(PayRunCheckpointView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedMargin + (double)parsedBatchSize;
    }
}
