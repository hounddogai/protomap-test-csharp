namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending refresh claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingRefreshClaimDesk(IPendingRefreshClaimStore Store, PendingRefreshClaimValidator Validator, IClock Clock, List<string> Journal, PendingRefreshClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending refresh claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingRefreshClaimStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending refresh claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingRefreshClaimStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending refresh claim {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of pending refresh claim workflow.</summary>
public static class PendingRefreshClaimWorkflow
{
    public static PendingRefreshClaimDesk NewPendingRefreshClaimDesk(IPendingRefreshClaimStore store, IClock clock, Registry flags)
    {
        return new PendingRefreshClaimDesk(
            store,
            PendingRefreshClaimRules.DefaultPendingRefreshClaimValidator(),
            clock,
            new List<string>(),
            PendingRefreshClaimEngine.DefaultPendingRefreshClaimEngine(flags));
    }
}
