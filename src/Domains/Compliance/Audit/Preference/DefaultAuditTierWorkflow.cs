namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default audit tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultAuditTierManager(IDefaultAuditTierStore Store, DefaultAuditTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default audit tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultAuditTierStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default audit tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultAuditTierStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of default audit tier workflow.</summary>
public static class DefaultAuditTierWorkflow
{
    public static DefaultAuditTierManager NewDefaultAuditTierManager(IDefaultAuditTierStore store, IClock clock)
    {
        return new DefaultAuditTierManager(
            store,
            DefaultAuditTierRules.DefaultDefaultAuditTierValidator(),
            clock,
            new List<string>());
    }
}
