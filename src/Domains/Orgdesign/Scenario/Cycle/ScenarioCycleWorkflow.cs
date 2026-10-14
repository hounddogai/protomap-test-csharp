namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scenario cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioCycleService(IScenarioCycleStore Store, ScenarioCycleValidator Validator, IClock Clock, List<string> Journal, ScenarioCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioCycleStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioCycleStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario cycle {code} not found");
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

/// <summary>Functions of scenario cycle workflow.</summary>
public static class ScenarioCycleWorkflow
{
    public static ScenarioCycleService NewScenarioCycleService(IScenarioCycleStore store, IClock clock)
    {
        return new ScenarioCycleService(
            store,
            ScenarioCycleRules.DefaultScenarioCycleValidator(),
            clock,
            new List<string>(),
            ScenarioCycleEngine.DefaultScenarioCycleEngine());
    }
}
