namespace Acme.Workforce.Orgdesign.Scenario.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scenario metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioMetricWorkflow(IScenarioMetricStore Store, ScenarioMetricValidator Validator, IClock Clock, List<string> Journal)
{
    public static ScenarioMetricWorkflow NewScenarioMetricWorkflow(IScenarioMetricStore store, IClock clock)
    {
        return new ScenarioMetricWorkflow(
            store,
            ScenarioMetricRules.DefaultScenarioMetricValidator(),
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
            throw new InvalidOperationException($"scenario metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioMetricStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioMetricStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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
            if (item.ExpiresAt > cutoff)
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
