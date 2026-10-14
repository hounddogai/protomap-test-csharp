namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual scenario windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualScenarioWindowManager(IManualScenarioWindowStore Store, ManualScenarioWindowValidator Validator, IClock Clock, List<string> Journal, ManualScenarioWindowEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual scenario window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualScenarioWindowStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual scenario window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualScenarioWindowStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual scenario window {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of manual scenario window workflow.</summary>
public static class ManualScenarioWindowWorkflow
{
    public static ManualScenarioWindowManager NewManualScenarioWindowManager(IManualScenarioWindowStore store, IClock clock)
    {
        return new ManualScenarioWindowManager(
            store,
            ManualScenarioWindowRules.DefaultManualScenarioWindowValidator(),
            clock,
            new List<string>(),
            ManualScenarioWindowEngine.DefaultManualScenarioWindowEngine());
    }
}
