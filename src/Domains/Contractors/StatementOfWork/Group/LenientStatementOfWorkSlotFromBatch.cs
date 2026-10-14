namespace Acme.Workforce.Contractors.StatementOfWork.Group;

using Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

/// <summary>Functions of lenient statement of work slot from batch.</summary>
public static class LenientStatementOfWorkSlotFromBatch
{
    public static Dictionary<string, double> SummarizeStatementOfWorkBatchesForSlot(List<StatementOfWorkBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<LenientStatementOfWorkSlot> DeriveLenientStatementOfWorkSlotsFromBatch(List<StatementOfWorkBatch> items)
    {
        var outValue = new List<LenientStatementOfWorkSlot>();
        foreach (var item in items)
        {
            var derived = LenientStatementOfWorkSlot.NewLenientStatementOfWorkSlot(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveLenientStatementOfWorkSlot(LenientStatementOfWorkSlot item)
    {
        return StatementOfWorkBatchPolicy.ClassifyStatementOfWorkBatch(item.ProjectedValue());
    }
}
