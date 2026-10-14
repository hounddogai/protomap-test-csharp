namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves desk quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskQuotaDesk(IDeskQuotaStore Store, DeskQuotaValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskQuotaStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskQuotaStatus.Failed);
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

/// <summary>Functions of desk quota workflow.</summary>
public static class DeskQuotaWorkflow
{
    public static DeskQuotaDesk NewDeskQuotaDesk(IDeskQuotaStore store, IClock clock)
    {
        return new DeskQuotaDesk(store, DeskQuotaRules.DefaultDeskQuotaValidator(), clock, new List<string>());
    }
}
