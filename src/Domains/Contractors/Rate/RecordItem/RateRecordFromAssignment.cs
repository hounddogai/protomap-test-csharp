namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Contractors.Engagement.Assignment;
using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Functions of rate record from assignment.</summary>
public static class RateRecordFromAssignment
{
    public static Dictionary<string, double> SummarizeStatementOfWorkAssignmentsForRecord(List<StatementOfWorkAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<RateRecord> DeriveRateRecordsFromAssignment(List<StatementOfWorkAssignment> items)
    {
        var outValue = new List<RateRecord>();
        foreach (var item in items)
        {
            var derived = RateRecord.NewRateRecord(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateRateRecord(RateRecord item)
    {
        return StatementOfWorkAssignmentPolicy.ClassifyStatementOfWorkAssignment(item.NetVolume());
    }

    public static Dictionary<string, double> SummarizeEngagementAssignmentsForRecord(List<EngagementAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<RateRecord> DeriveRateRecordsFromAssignmentExtra(List<EngagementAssignment> items)
    {
        var outValue = new List<RateRecord>();
        foreach (var item in items)
        {
            var derived = RateRecord.NewRateRecord(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateRateRecordExtra(RateRecord item)
    {
        return EngagementAssignmentPolicy.EngagementAssignmentBucket(item.NetVolume());
    }
}
