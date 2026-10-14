namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global scenario requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalScenarioRequestDesk(IGlobalScenarioRequestStore Store, GlobalScenarioRequestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global scenario request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalScenarioRequestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global scenario request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalScenarioRequestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of global scenario request workflow.</summary>
public static class GlobalScenarioRequestWorkflow
{
    public static GlobalScenarioRequestDesk NewGlobalScenarioRequestDesk(IGlobalScenarioRequestStore store, IClock clock)
    {
        return new GlobalScenarioRequestDesk(
            store,
            GlobalScenarioRequestRules.DefaultGlobalScenarioRequestValidator(),
            clock,
            new List<string>());
    }
}
