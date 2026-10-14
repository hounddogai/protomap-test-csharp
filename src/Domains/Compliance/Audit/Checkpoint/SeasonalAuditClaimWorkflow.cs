namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal audit claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalAuditClaimCoordinator(ISeasonalAuditClaimStore Store, SeasonalAuditClaimValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal audit claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalAuditClaimStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal audit claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalAuditClaimStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of seasonal audit claim workflow.</summary>
public static class SeasonalAuditClaimWorkflow
{
    public static SeasonalAuditClaimCoordinator NewSeasonalAuditClaimCoordinator(ISeasonalAuditClaimStore store, IClock clock)
    {
        return new SeasonalAuditClaimCoordinator(
            store,
            SeasonalAuditClaimRules.DefaultSeasonalAuditClaimValidator(),
            clock,
            new List<string>());
    }
}
