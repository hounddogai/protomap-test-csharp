namespace Acme.Workforce.Compensation.EquityGrant.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled equity grant plans through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledEquityGrantPlanDesk(IScheduledEquityGrantPlanStore Store, ScheduledEquityGrantPlanValidator Validator, IClock Clock, List<string> Journal, ScheduledEquityGrantPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled equity grant plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledEquityGrantPlanStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled equity grant plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledEquityGrantPlanStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled equity grant plan {code} not found");
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

/// <summary>Functions of scheduled equity grant plan workflow.</summary>
public static class ScheduledEquityGrantPlanWorkflow
{
    public static ScheduledEquityGrantPlanDesk NewScheduledEquityGrantPlanDesk(IScheduledEquityGrantPlanStore store, IClock clock, Registry flags)
    {
        return new ScheduledEquityGrantPlanDesk(
            store,
            ScheduledEquityGrantPlanRules.DefaultScheduledEquityGrantPlanValidator(),
            clock,
            new List<string>(),
            ScheduledEquityGrantPlanEngine.DefaultScheduledEquityGrantPlanEngine(flags));
    }
}
