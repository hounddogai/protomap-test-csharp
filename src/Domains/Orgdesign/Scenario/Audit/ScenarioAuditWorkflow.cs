namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scenario audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioAuditDesk(IScenarioAuditStore Store, ScenarioAuditValidator Validator, IClock Clock, List<string> Journal, ScenarioAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioAuditStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioAuditStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario audit {code} not found");
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

/// <summary>Functions of scenario audit workflow.</summary>
public static class ScenarioAuditWorkflow
{
    public static ScenarioAuditDesk NewScenarioAuditDesk(IScenarioAuditStore store, IClock clock)
    {
        return new ScenarioAuditDesk(
            store,
            ScenarioAuditRules.DefaultScenarioAuditValidator(),
            clock,
            new List<string>(),
            ScenarioAuditEngine.DefaultScenarioAuditEngine());
    }
}
