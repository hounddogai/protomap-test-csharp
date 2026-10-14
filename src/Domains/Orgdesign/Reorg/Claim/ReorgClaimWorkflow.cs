namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves reorg claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReorgClaimDesk(IReorgClaimStore Store, ReorgClaimValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReorgClaimStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReorgClaimStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of reorg claim workflow.</summary>
public static class ReorgClaimWorkflow
{
    public static ReorgClaimDesk NewReorgClaimDesk(IReorgClaimStore store, IClock clock)
    {
        return new ReorgClaimDesk(store, ReorgClaimRules.DefaultReorgClaimValidator(), clock, new List<string>());
    }
}
