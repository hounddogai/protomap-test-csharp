namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled garnishment plans through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledGarnishmentPlanManager(IScheduledGarnishmentPlanStore Store, ScheduledGarnishmentPlanValidator Validator, IClock Clock, List<string> Journal, ScheduledGarnishmentPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled garnishment plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledGarnishmentPlanStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled garnishment plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledGarnishmentPlanStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled garnishment plan {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of scheduled garnishment plan workflow.</summary>
public static class ScheduledGarnishmentPlanWorkflow
{
    public static ScheduledGarnishmentPlanManager NewScheduledGarnishmentPlanManager(IScheduledGarnishmentPlanStore store, IClock clock, Registry flags)
    {
        return new ScheduledGarnishmentPlanManager(
            store,
            ScheduledGarnishmentPlanRules.DefaultScheduledGarnishmentPlanValidator(),
            clock,
            new List<string>(),
            ScheduledGarnishmentPlanEngine.DefaultScheduledGarnishmentPlanEngine(flags));
    }
}
