namespace Acme.Workforce.Contractors.Engagement.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped engagement entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedEngagementEntryCoordinator(ICappedEngagementEntryStore Store, CappedEngagementEntryValidator Validator, IClock Clock, List<string> Journal, CappedEngagementEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped engagement entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedEngagementEntryStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped engagement entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedEngagementEntryStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped engagement entry {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of capped engagement entry workflow.</summary>
public static class CappedEngagementEntryWorkflow
{
    public static CappedEngagementEntryCoordinator NewCappedEngagementEntryCoordinator(ICappedEngagementEntryStore store, IClock clock)
    {
        return new CappedEngagementEntryCoordinator(
            store,
            CappedEngagementEntryRules.DefaultCappedEngagementEntryValidator(),
            clock,
            new List<string>(),
            CappedEngagementEntryEngine.DefaultCappedEngagementEntryEngine());
    }
}
