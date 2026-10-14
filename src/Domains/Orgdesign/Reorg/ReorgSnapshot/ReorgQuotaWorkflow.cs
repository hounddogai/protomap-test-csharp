namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves reorg quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReorgQuotaCoordinator(IReorgQuotaStore Store, ReorgQuotaValidator Validator, IClock Clock, List<string> Journal, ReorgQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReorgQuotaStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReorgQuotaStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg quota {code} not found");
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

/// <summary>Functions of reorg quota workflow.</summary>
public static class ReorgQuotaWorkflow
{
    public static ReorgQuotaCoordinator NewReorgQuotaCoordinator(IReorgQuotaStore store, IClock clock, Registry flags)
    {
        return new ReorgQuotaCoordinator(
            store,
            ReorgQuotaRules.DefaultReorgQuotaValidator(),
            clock,
            new List<string>(),
            ReorgQuotaEngine.DefaultReorgQuotaEngine(flags));
    }
}
