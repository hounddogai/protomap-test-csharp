namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual scenario estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualScenarioEstimateWorkflow(IManualScenarioEstimateStore Store, ManualScenarioEstimateValidator Validator, IClock Clock, List<string> Journal)
{
    public static ManualScenarioEstimateWorkflow NewManualScenarioEstimateWorkflow(IManualScenarioEstimateStore store, IClock clock)
    {
        return new ManualScenarioEstimateWorkflow(
            store,
            ManualScenarioEstimateRules.DefaultManualScenarioEstimateValidator(),
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
            throw new InvalidOperationException($"manual scenario estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualScenarioEstimateStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual scenario estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualScenarioEstimateStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public int ReviewStale(int cutoff)
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
