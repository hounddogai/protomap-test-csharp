namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scenario rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioRuleDesk(IScenarioRuleStore Store, ScenarioRuleValidator Validator, IClock Clock, List<string> Journal, ScenarioRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioRuleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioRuleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario rule {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of scenario rule workflow.</summary>
public static class ScenarioRuleWorkflow
{
    public static ScenarioRuleDesk NewScenarioRuleDesk(IScenarioRuleStore store, IClock clock)
    {
        return new ScenarioRuleDesk(
            store,
            ScenarioRuleRules.DefaultScenarioRuleValidator(),
            clock,
            new List<string>(),
            ScenarioRuleEngine.DefaultScenarioRuleEngine());
    }
}
