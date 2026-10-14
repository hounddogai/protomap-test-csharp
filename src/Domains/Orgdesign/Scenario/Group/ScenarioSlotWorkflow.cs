namespace Acme.Workforce.Orgdesign.Scenario.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scenario slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioSlotCoordinator(IScenarioSlotStore Store, ScenarioSlotValidator Validator, IClock Clock, List<string> Journal, ScenarioSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioSlotStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioSlotStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario slot {code} not found");
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

/// <summary>Functions of scenario slot workflow.</summary>
public static class ScenarioSlotWorkflow
{
    public static ScenarioSlotCoordinator NewScenarioSlotCoordinator(IScenarioSlotStore store, IClock clock)
    {
        return new ScenarioSlotCoordinator(
            store,
            ScenarioSlotRules.DefaultScenarioSlotValidator(),
            clock,
            new List<string>(),
            ScenarioSlotEngine.DefaultScenarioSlotEngine());
    }
}
