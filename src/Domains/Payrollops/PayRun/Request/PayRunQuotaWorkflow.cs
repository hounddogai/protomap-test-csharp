namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pay run quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayRunQuotaManager(IPayRunQuotaStore Store, PayRunQuotaValidator Validator, IClock Clock, List<string> Journal, PayRunQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayRunQuotaStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayRunQuotaStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run quota {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of pay run quota workflow.</summary>
public static class PayRunQuotaWorkflow
{
    public static PayRunQuotaManager NewPayRunQuotaManager(IPayRunQuotaStore store, IClock clock, Registry flags)
    {
        return new PayRunQuotaManager(
            store,
            PayRunQuotaRules.DefaultPayRunQuotaValidator(),
            clock,
            new List<string>(),
            PayRunQuotaEngine.DefaultPayRunQuotaEngine(flags));
    }
}
