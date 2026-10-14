namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scenario batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioBatchService(IScenarioBatchStore Store, ScenarioBatchValidator Validator, IClock Clock, List<string> Journal, ScenarioBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioBatchStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioBatchStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario batch {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of scenario batch workflow.</summary>
public static class ScenarioBatchWorkflow
{
    public static ScenarioBatchService NewScenarioBatchService(IScenarioBatchStore store, IClock clock)
    {
        return new ScenarioBatchService(
            store,
            ScenarioBatchRules.DefaultScenarioBatchValidator(),
            clock,
            new List<string>(),
            ScenarioBatchEngine.DefaultScenarioBatchEngine());
    }
}
