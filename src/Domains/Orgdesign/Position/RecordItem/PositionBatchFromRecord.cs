namespace Acme.Workforce.Orgdesign.Position.RecordItem;

using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Functions of position batch from record.</summary>
public static class PositionBatchFromRecord
{
    public static Dictionary<string, double> SummarizeSpanRecordsForBatch(List<SpanRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<PositionBatch> DerivePositionBatchesFromRecord(List<SpanRecord> items)
    {
        var outValue = new List<PositionBatch>();
        foreach (var item in items)
        {
            var derived = PositionBatch.NewPositionBatch(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcilePositionBatch(PositionBatch item)
    {
        return SpanRecordPolicy.ClassifySpanRecord(item.ProjectedValue());
    }
}
