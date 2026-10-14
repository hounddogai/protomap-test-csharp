namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback license seat audits through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackLicenseSeatAuditManager(IFallbackLicenseSeatAuditStore Store, FallbackLicenseSeatAuditValidator Validator, IClock Clock, List<string> Journal, FallbackLicenseSeatAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback license seat audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackLicenseSeatAuditStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback license seat audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackLicenseSeatAuditStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback license seat audit {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of fallback license seat audit workflow.</summary>
public static class FallbackLicenseSeatAuditWorkflow
{
    public static FallbackLicenseSeatAuditManager NewFallbackLicenseSeatAuditManager(IFallbackLicenseSeatAuditStore store, IClock clock)
    {
        return new FallbackLicenseSeatAuditManager(
            store,
            FallbackLicenseSeatAuditRules.DefaultFallbackLicenseSeatAuditValidator(),
            clock,
            new List<string>(),
            FallbackLicenseSeatAuditEngine.DefaultFallbackLicenseSeatAuditEngine());
    }
}
