namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scenario segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioSegmentManager(IScenarioSegmentStore Store, ScenarioSegmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioSegmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioSegmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of scenario segment workflow.</summary>
public static class ScenarioSegmentWorkflow
{
    public static ScenarioSegmentManager NewScenarioSegmentManager(IScenarioSegmentStore store, IClock clock)
    {
        return new ScenarioSegmentManager(
            store,
            ScenarioSegmentRules.DefaultScenarioSegmentValidator(),
            clock,
            new List<string>());
    }
}
