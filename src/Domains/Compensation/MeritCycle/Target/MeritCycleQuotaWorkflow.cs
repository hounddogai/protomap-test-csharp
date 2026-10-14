namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves merit cycle quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleQuotaManager(IMeritCycleQuotaStore Store, MeritCycleQuotaValidator Validator, IClock Clock, List<string> Journal, MeritCycleQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleQuotaStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleQuotaStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle quota {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of merit cycle quota workflow.</summary>
public static class MeritCycleQuotaWorkflow
{
    public static MeritCycleQuotaManager NewMeritCycleQuotaManager(IMeritCycleQuotaStore store, IClock clock, Registry flags)
    {
        return new MeritCycleQuotaManager(
            store,
            MeritCycleQuotaRules.DefaultMeritCycleQuotaValidator(),
            clock,
            new List<string>(),
            MeritCycleQuotaEngine.DefaultMeritCycleQuotaEngine(flags));
    }
}
