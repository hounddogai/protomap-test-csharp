namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual scenario rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualScenarioRuleWorkflow(IManualScenarioRuleStore Store, ManualScenarioRuleValidator Validator, IClock Clock, List<string> Journal)
{
    public static ManualScenarioRuleWorkflow NewManualScenarioRuleWorkflow(IManualScenarioRuleStore store, IClock clock)
    {
        return new ManualScenarioRuleWorkflow(
            store,
            ManualScenarioRuleRules.DefaultManualScenarioRuleValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual scenario rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualScenarioRuleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual scenario rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualScenarioRuleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
