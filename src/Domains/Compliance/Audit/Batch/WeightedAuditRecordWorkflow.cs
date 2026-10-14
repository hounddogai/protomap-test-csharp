namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted audit records through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedAuditRecordManager(IWeightedAuditRecordStore Store, WeightedAuditRecordValidator Validator, IClock Clock, List<string> Journal, WeightedAuditRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted audit record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedAuditRecordStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted audit record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedAuditRecordStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted audit record {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of weighted audit record workflow.</summary>
public static class WeightedAuditRecordWorkflow
{
    public static WeightedAuditRecordManager NewWeightedAuditRecordManager(IWeightedAuditRecordStore store, IClock clock)
    {
        return new WeightedAuditRecordManager(
            store,
            WeightedAuditRecordRules.DefaultWeightedAuditRecordValidator(),
            clock,
            new List<string>(),
            WeightedAuditRecordEngine.DefaultWeightedAuditRecordEngine());
    }
}
