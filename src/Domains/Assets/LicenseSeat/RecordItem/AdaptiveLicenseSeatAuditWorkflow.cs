namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive license seat audits through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveLicenseSeatAuditService(IAdaptiveLicenseSeatAuditStore Store, AdaptiveLicenseSeatAuditValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive license seat audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveLicenseSeatAuditStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive license seat audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveLicenseSeatAuditStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of adaptive license seat audit workflow.</summary>
public static class AdaptiveLicenseSeatAuditWorkflow
{
    public static AdaptiveLicenseSeatAuditService NewAdaptiveLicenseSeatAuditService(IAdaptiveLicenseSeatAuditStore store, IClock clock)
    {
        return new AdaptiveLicenseSeatAuditService(
            store,
            AdaptiveLicenseSeatAuditRules.DefaultAdaptiveLicenseSeatAuditValidator(),
            clock,
            new List<string>());
    }
}
