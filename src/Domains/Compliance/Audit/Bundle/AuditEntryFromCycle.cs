namespace Acme.Workforce.Compliance.Audit.Bundle;

using Acme.Workforce.Compliance.Exception.Cycle;

/// <summary>Functions of audit entry from cycle.</summary>
public static class AuditEntryFromCycle
{
    public static Dictionary<string, double> SummarizeExceptionCyclesForEntry(List<ExceptionCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<AuditEntry> DeriveAuditEntriesFromCycle(List<ExceptionCycle> items)
    {
        var outValue = new List<AuditEntry>();
        foreach (var item in items)
        {
            var derived = AuditEntry.NewAuditEntry(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeAuditEntry(AuditEntry item)
    {
        return ExceptionCyclePolicy.NormalizeExceptionCycleCode(item.Code);
    }
}
