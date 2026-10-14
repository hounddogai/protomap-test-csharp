namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scenario targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioTargetWorkflow(IScenarioTargetStore Store, ScenarioTargetValidator Validator, IClock Clock, List<string> Journal, ScenarioTargetEngine Engine)
{
    public static ScenarioTargetWorkflow NewScenarioTargetWorkflow(IScenarioTargetStore store, IClock clock)
    {
        return new ScenarioTargetWorkflow(
            store,
            ScenarioTargetRules.DefaultScenarioTargetValidator(),
            clock,
            new List<string>(),
            ScenarioTargetEngine.DefaultScenarioTargetEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioTargetStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioTargetStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario target {code} not found");
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
