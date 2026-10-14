namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered merit cycle plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredMeritCyclePlanDesk(ITieredMeritCyclePlanStore Store, TieredMeritCyclePlanValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered merit cycle plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredMeritCyclePlanStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered merit cycle plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredMeritCyclePlanStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of tiered merit cycle plan workflow.</summary>
public static class TieredMeritCyclePlanWorkflow
{
    public static TieredMeritCyclePlanDesk NewTieredMeritCyclePlanDesk(ITieredMeritCyclePlanStore store, IClock clock)
    {
        return new TieredMeritCyclePlanDesk(
            store,
            TieredMeritCyclePlanRules.DefaultTieredMeritCyclePlanValidator(),
            clock,
            new List<string>());
    }
}
