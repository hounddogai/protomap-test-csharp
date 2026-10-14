namespace Acme.Workforce.Payrollops.Deduction.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling deduction entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingDeductionEntryWorkflow(IRollingDeductionEntryStore Store, RollingDeductionEntryValidator Validator, IClock Clock, List<string> Journal)
{
    public static RollingDeductionEntryWorkflow NewRollingDeductionEntryWorkflow(IRollingDeductionEntryStore store, IClock clock)
    {
        return new RollingDeductionEntryWorkflow(
            store,
            RollingDeductionEntryRules.DefaultRollingDeductionEntryValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling deduction entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingDeductionEntryStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling deduction entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingDeductionEntryStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
