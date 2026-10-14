namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending pay band quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingPayBandQuotaManager(IPendingPayBandQuotaStore Store, PendingPayBandQuotaValidator Validator, IClock Clock, List<string> Journal, PendingPayBandQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pay band quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingPayBandQuotaStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pay band quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingPayBandQuotaStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pay band quota {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of pending pay band quota workflow.</summary>
public static class PendingPayBandQuotaWorkflow
{
    public static PendingPayBandQuotaManager NewPendingPayBandQuotaManager(IPendingPayBandQuotaStore store, IClock clock)
    {
        return new PendingPayBandQuotaManager(
            store,
            PendingPayBandQuotaRules.DefaultPendingPayBandQuotaValidator(),
            clock,
            new List<string>(),
            PendingPayBandQuotaEngine.DefaultPendingPayBandQuotaEngine());
    }
}
