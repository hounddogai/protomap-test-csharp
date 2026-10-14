namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling merit cycle records through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingMeritCycleRecordCoordinator(IRollingMeritCycleRecordStore Store, RollingMeritCycleRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling merit cycle record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingMeritCycleRecordStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling merit cycle record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingMeritCycleRecordStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of rolling merit cycle record workflow.</summary>
public static class RollingMeritCycleRecordWorkflow
{
    public static RollingMeritCycleRecordCoordinator NewRollingMeritCycleRecordCoordinator(IRollingMeritCycleRecordStore store, IClock clock)
    {
        return new RollingMeritCycleRecordCoordinator(
            store,
            RollingMeritCycleRecordRules.DefaultRollingMeritCycleRecordValidator(),
            clock,
            new List<string>());
    }
}
