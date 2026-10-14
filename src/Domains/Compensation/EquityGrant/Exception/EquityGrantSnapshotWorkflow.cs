namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves equity grant snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record EquityGrantSnapshotWorkflow(IEquityGrantSnapshotStore Store, EquityGrantSnapshotValidator Validator, IClock Clock, List<string> Journal, EquityGrantSnapshotEngine Engine)
{
    public static EquityGrantSnapshotWorkflow NewEquityGrantSnapshotWorkflow(IEquityGrantSnapshotStore store, IClock clock)
    {
        return new EquityGrantSnapshotWorkflow(
            store,
            EquityGrantSnapshotRules.DefaultEquityGrantSnapshotValidator(),
            clock,
            new List<string>(),
            EquityGrantSnapshotEngine.DefaultEquityGrantSnapshotEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EquityGrantSnapshotStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EquityGrantSnapshotStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant snapshot {code} not found");
        }
        var result = Engine.Evaluate(item);
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
