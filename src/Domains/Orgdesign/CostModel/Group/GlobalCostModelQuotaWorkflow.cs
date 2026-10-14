namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global cost model quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalCostModelQuotaManager(IGlobalCostModelQuotaStore Store, GlobalCostModelQuotaValidator Validator, IClock Clock, List<string> Journal, GlobalCostModelQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global cost model quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalCostModelQuotaStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global cost model quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalCostModelQuotaStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global cost model quota {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of global cost model quota workflow.</summary>
public static class GlobalCostModelQuotaWorkflow
{
    public static GlobalCostModelQuotaManager NewGlobalCostModelQuotaManager(IGlobalCostModelQuotaStore store, IClock clock, Registry flags)
    {
        return new GlobalCostModelQuotaManager(
            store,
            GlobalCostModelQuotaRules.DefaultGlobalCostModelQuotaValidator(),
            clock,
            new List<string>(),
            GlobalCostModelQuotaEngine.DefaultGlobalCostModelQuotaEngine(flags));
    }
}
