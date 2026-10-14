namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayQuotaManager(IRetroPayQuotaStore Store, RetroPayQuotaValidator Validator, IClock Clock, List<string> Journal, RetroPayQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayQuotaStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayQuotaStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay quota {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of retro pay quota workflow.</summary>
public static class RetroPayQuotaWorkflow
{
    public static RetroPayQuotaManager NewRetroPayQuotaManager(IRetroPayQuotaStore store, IClock clock)
    {
        return new RetroPayQuotaManager(
            store,
            RetroPayQuotaRules.DefaultRetroPayQuotaValidator(),
            clock,
            new List<string>(),
            RetroPayQuotaEngine.DefaultRetroPayQuotaEngine());
    }
}
