namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scenario checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioCheckpointManager(IScenarioCheckpointStore Store, ScenarioCheckpointValidator Validator, IClock Clock, List<string> Journal, ScenarioCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioCheckpointStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioCheckpointStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario checkpoint {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of scenario checkpoint workflow.</summary>
public static class ScenarioCheckpointWorkflow
{
    public static ScenarioCheckpointManager NewScenarioCheckpointManager(IScenarioCheckpointStore store, IClock clock)
    {
        return new ScenarioCheckpointManager(
            store,
            ScenarioCheckpointRules.DefaultScenarioCheckpointValidator(),
            clock,
            new List<string>(),
            ScenarioCheckpointEngine.DefaultScenarioCheckpointEngine());
    }
}
