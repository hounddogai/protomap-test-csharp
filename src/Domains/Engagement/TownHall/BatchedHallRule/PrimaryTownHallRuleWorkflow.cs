namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary town hall rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryTownHallRuleManager(IPrimaryTownHallRuleStore Store, PrimaryTownHallRuleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary town hall rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryTownHallRuleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary town hall rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryTownHallRuleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of primary town hall rule workflow.</summary>
public static class PrimaryTownHallRuleWorkflow
{
    public static PrimaryTownHallRuleManager NewPrimaryTownHallRuleManager(IPrimaryTownHallRuleStore store, IClock clock)
    {
        return new PrimaryTownHallRuleManager(
            store,
            PrimaryTownHallRuleRules.DefaultPrimaryTownHallRuleValidator(),
            clock,
            new List<string>());
    }
}
