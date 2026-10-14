namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayAuditWorkflow(IRetroPayAuditStore Store, RetroPayAuditValidator Validator, IClock Clock, List<string> Journal)
{
    public static RetroPayAuditWorkflow NewRetroPayAuditWorkflow(IRetroPayAuditStore store, IClock clock)
    {
        return new RetroPayAuditWorkflow(
            store,
            RetroPayAuditRules.DefaultRetroPayAuditValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayAuditStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayAuditStatus.Closed);
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
            if (item.StartsAt > cutoff)
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
