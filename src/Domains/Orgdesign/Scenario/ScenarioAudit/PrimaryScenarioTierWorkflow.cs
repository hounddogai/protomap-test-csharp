namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary scenario tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryScenarioTierDesk(IPrimaryScenarioTierStore Store, PrimaryScenarioTierValidator Validator, IClock Clock, List<string> Journal, PrimaryScenarioTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary scenario tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryScenarioTierStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary scenario tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryScenarioTierStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary scenario tier {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of primary scenario tier workflow.</summary>
public static class PrimaryScenarioTierWorkflow
{
    public static PrimaryScenarioTierDesk NewPrimaryScenarioTierDesk(IPrimaryScenarioTierStore store, IClock clock)
    {
        return new PrimaryScenarioTierDesk(
            store,
            PrimaryScenarioTierRules.DefaultPrimaryScenarioTierValidator(),
            clock,
            new List<string>(),
            PrimaryScenarioTierEngine.DefaultPrimaryScenarioTierEngine());
    }
}
