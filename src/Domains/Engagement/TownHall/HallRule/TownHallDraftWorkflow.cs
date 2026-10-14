namespace Acme.Workforce.Engagement.TownHall.HallRule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves town hall drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallDraftDesk(ITownHallDraftStore Store, TownHallDraftValidator Validator, IClock Clock, List<string> Journal, TownHallDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallDraftStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallDraftStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall draft {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of town hall draft workflow.</summary>
public static class TownHallDraftWorkflow
{
    public static TownHallDraftDesk NewTownHallDraftDesk(ITownHallDraftStore store, IClock clock)
    {
        return new TownHallDraftDesk(
            store,
            TownHallDraftRules.DefaultTownHallDraftValidator(),
            clock,
            new List<string>(),
            TownHallDraftEngine.DefaultTownHallDraftEngine());
    }
}
