namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global scenario slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalScenarioSlotManager(IGlobalScenarioSlotStore Store, GlobalScenarioSlotValidator Validator, IClock Clock, List<string> Journal, GlobalScenarioSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global scenario slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalScenarioSlotStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global scenario slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalScenarioSlotStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global scenario slot {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of global scenario slot workflow.</summary>
public static class GlobalScenarioSlotWorkflow
{
    public static GlobalScenarioSlotManager NewGlobalScenarioSlotManager(IGlobalScenarioSlotStore store, IClock clock)
    {
        return new GlobalScenarioSlotManager(
            store,
            GlobalScenarioSlotRules.DefaultGlobalScenarioSlotValidator(),
            clock,
            new List<string>(),
            GlobalScenarioSlotEngine.DefaultGlobalScenarioSlotEngine());
    }
}
