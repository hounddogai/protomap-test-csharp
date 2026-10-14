namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending exception quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingExceptionQuotaDesk(IPendingExceptionQuotaStore Store, PendingExceptionQuotaValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending exception quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingExceptionQuotaStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending exception quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingExceptionQuotaStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of pending exception quota workflow.</summary>
public static class PendingExceptionQuotaWorkflow
{
    public static PendingExceptionQuotaDesk NewPendingExceptionQuotaDesk(IPendingExceptionQuotaStore store, IClock clock)
    {
        return new PendingExceptionQuotaDesk(
            store,
            PendingExceptionQuotaRules.DefaultPendingExceptionQuotaValidator(),
            clock,
            new List<string>());
    }
}
