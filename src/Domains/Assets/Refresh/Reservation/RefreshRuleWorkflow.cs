namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves refresh rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshRuleDesk(IRefreshRuleStore Store, RefreshRuleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshRuleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshRuleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of refresh rule workflow.</summary>
public static class RefreshRuleWorkflow
{
    public static RefreshRuleDesk NewRefreshRuleDesk(IRefreshRuleStore store, IClock clock)
    {
        return new RefreshRuleDesk(store, RefreshRuleRules.DefaultRefreshRuleValidator(), clock, new List<string>());
    }
}
