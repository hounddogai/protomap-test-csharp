namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves merit cycle baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleBaselineManager(IMeritCycleBaselineStore Store, MeritCycleBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleBaselineStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleBaselineStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of merit cycle baseline workflow.</summary>
public static class MeritCycleBaselineWorkflow
{
    public static MeritCycleBaselineManager NewMeritCycleBaselineManager(IMeritCycleBaselineStore store, IClock clock)
    {
        return new MeritCycleBaselineManager(
            store,
            MeritCycleBaselineRules.DefaultMeritCycleBaselineValidator(),
            clock,
            new List<string>());
    }
}
