namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scenario agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioAgreementCoordinator(IScenarioAgreementStore Store, ScenarioAgreementValidator Validator, IClock Clock, List<string> Journal, ScenarioAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioAgreementStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioAgreementStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario agreement {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of scenario agreement workflow.</summary>
public static class ScenarioAgreementWorkflow
{
    public static ScenarioAgreementCoordinator NewScenarioAgreementCoordinator(IScenarioAgreementStore store, IClock clock)
    {
        return new ScenarioAgreementCoordinator(
            store,
            ScenarioAgreementRules.DefaultScenarioAgreementValidator(),
            clock,
            new List<string>(),
            ScenarioAgreementEngine.DefaultScenarioAgreementEngine());
    }
}
