namespace Acme.Workforce.Assets.Refresh.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled refresh mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledRefreshMappingWorkflow(IScheduledRefreshMappingStore Store, ScheduledRefreshMappingValidator Validator, IClock Clock, List<string> Journal, ScheduledRefreshMappingEngine Engine)
{
    public static ScheduledRefreshMappingWorkflow NewScheduledRefreshMappingWorkflow(IScheduledRefreshMappingStore store, IClock clock)
    {
        return new ScheduledRefreshMappingWorkflow(
            store,
            ScheduledRefreshMappingRules.DefaultScheduledRefreshMappingValidator(),
            clock,
            new List<string>(),
            ScheduledRefreshMappingEngine.DefaultScheduledRefreshMappingEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled refresh mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledRefreshMappingStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled refresh mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledRefreshMappingStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled refresh mapping {code} not found");
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
