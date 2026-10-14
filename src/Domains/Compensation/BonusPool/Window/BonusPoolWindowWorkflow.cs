namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves bonus pool windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolWindowService(IBonusPoolWindowStore Store, BonusPoolWindowValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolWindowStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolWindowStatus.Expired);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of bonus pool window workflow.</summary>
public static class BonusPoolWindowWorkflow
{
    public static BonusPoolWindowService NewBonusPoolWindowService(IBonusPoolWindowStore store, IClock clock)
    {
        return new BonusPoolWindowService(
            store,
            BonusPoolWindowRules.DefaultBonusPoolWindowValidator(),
            clock,
            new List<string>());
    }
}
