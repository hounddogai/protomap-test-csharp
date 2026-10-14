namespace Acme.Workforce.Compensation.EquityGrant.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled equity grant quotas through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledEquityGrantQuotaService(IScheduledEquityGrantQuotaStore Store, ScheduledEquityGrantQuotaValidator Validator, IClock Clock, List<string> Journal, ScheduledEquityGrantQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled equity grant quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledEquityGrantQuotaStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled equity grant quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledEquityGrantQuotaStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled equity grant quota {code} not found");
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

/// <summary>Functions of scheduled equity grant quota workflow.</summary>
public static class ScheduledEquityGrantQuotaWorkflow
{
    public static ScheduledEquityGrantQuotaService NewScheduledEquityGrantQuotaService(IScheduledEquityGrantQuotaStore store, IClock clock, Registry flags)
    {
        return new ScheduledEquityGrantQuotaService(
            store,
            ScheduledEquityGrantQuotaRules.DefaultScheduledEquityGrantQuotaValidator(),
            clock,
            new List<string>(),
            ScheduledEquityGrantQuotaEngine.DefaultScheduledEquityGrantQuotaEngine(flags));
    }
}
