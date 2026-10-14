namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped engagement agreements through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedEngagementAgreementCoordinator(ICappedEngagementAgreementStore Store, CappedEngagementAgreementValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped engagement agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedEngagementAgreementStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped engagement agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedEngagementAgreementStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of capped engagement agreement workflow.</summary>
public static class CappedEngagementAgreementWorkflow
{
    public static CappedEngagementAgreementCoordinator NewCappedEngagementAgreementCoordinator(ICappedEngagementAgreementStore store, IClock clock)
    {
        return new CappedEngagementAgreementCoordinator(
            store,
            CappedEngagementAgreementRules.DefaultCappedEngagementAgreementValidator(),
            clock,
            new List<string>());
    }
}
