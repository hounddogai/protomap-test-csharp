namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending pay band digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingPayBandDigestService(IPendingPayBandDigestStore Store, PendingPayBandDigestValidator Validator, IClock Clock, List<string> Journal, PendingPayBandDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pay band digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingPayBandDigestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pay band digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingPayBandDigestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pay band digest {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of pending pay band digest workflow.</summary>
public static class PendingPayBandDigestWorkflow
{
    public static PendingPayBandDigestService NewPendingPayBandDigestService(IPendingPayBandDigestStore store, IClock clock, Registry flags)
    {
        return new PendingPayBandDigestService(
            store,
            PendingPayBandDigestRules.DefaultPendingPayBandDigestValidator(),
            clock,
            new List<string>(),
            PendingPayBandDigestEngine.DefaultPendingPayBandDigestEngine(flags));
    }
}
