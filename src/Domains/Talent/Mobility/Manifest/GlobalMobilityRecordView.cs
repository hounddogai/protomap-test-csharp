namespace Acme.Workforce.Talent.Mobility.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global mobility record.</summary>
public sealed record GlobalMobilityRecordView(string Code, string Title, string Amount, string BatchSize, string Reviewer, string ManagerId, string ExpiresAtText, string StatusLabel)
{
    public static GlobalMobilityRecordView ToGlobalMobilityRecordView(GlobalMobilityRecord item)
    {
        return new GlobalMobilityRecordView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.ManagerId,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalMobilityRecordView> ToGlobalMobilityRecordViews(List<GlobalMobilityRecord> items)
    {
        var views = new List<GlobalMobilityRecordView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalMobilityRecordView(item));
        }
        return views;
    }

    public static double SumGlobalMobilityRecordView(GlobalMobilityRecordView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedBatchSize + (double)parsedExpiresAt;
    }
}
