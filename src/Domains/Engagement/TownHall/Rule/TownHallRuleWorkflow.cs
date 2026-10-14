namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves town hall rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallRuleWorkflow(ITownHallRuleStore Store, TownHallRuleValidator Validator, IClock Clock, List<string> Journal)
{
    public static TownHallRuleWorkflow NewTownHallRuleWorkflow(ITownHallRuleStore store, IClock clock)
    {
        return new TownHallRuleWorkflow(
            store,
            TownHallRuleRules.DefaultTownHallRuleValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallRuleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallRuleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
