namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered engagement agreements through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredEngagementAgreementCoordinator(ITieredEngagementAgreementStore Store, TieredEngagementAgreementValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered engagement agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredEngagementAgreementStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered engagement agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredEngagementAgreementStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of tiered engagement agreement workflow.</summary>
public static class TieredEngagementAgreementWorkflow
{
    public static TieredEngagementAgreementCoordinator NewTieredEngagementAgreementCoordinator(ITieredEngagementAgreementStore store, IClock clock)
    {
        return new TieredEngagementAgreementCoordinator(
            store,
            TieredEngagementAgreementRules.DefaultTieredEngagementAgreementValidator(),
            clock,
            new List<string>());
    }
}
