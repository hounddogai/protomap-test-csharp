namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves merit cycle thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleThresholdService(IMeritCycleThresholdStore Store, MeritCycleThresholdValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleThresholdStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleThresholdStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of merit cycle threshold workflow.</summary>
public static class MeritCycleThresholdWorkflow
{
    public static MeritCycleThresholdService NewMeritCycleThresholdService(IMeritCycleThresholdStore store, IClock clock)
    {
        return new MeritCycleThresholdService(
            store,
            MeritCycleThresholdRules.DefaultMeritCycleThresholdValidator(),
            clock,
            new List<string>());
    }
}
