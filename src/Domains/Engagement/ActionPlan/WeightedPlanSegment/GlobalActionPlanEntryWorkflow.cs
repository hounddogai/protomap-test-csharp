namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global action plan entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalActionPlanEntryService(IGlobalActionPlanEntryStore Store, GlobalActionPlanEntryValidator Validator, IClock Clock, List<string> Journal, GlobalActionPlanEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global action plan entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalActionPlanEntryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global action plan entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalActionPlanEntryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global action plan entry {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of global action plan entry workflow.</summary>
public static class GlobalActionPlanEntryWorkflow
{
    public static GlobalActionPlanEntryService NewGlobalActionPlanEntryService(IGlobalActionPlanEntryStore store, IClock clock, Registry flags)
    {
        return new GlobalActionPlanEntryService(
            store,
            GlobalActionPlanEntryRules.DefaultGlobalActionPlanEntryValidator(),
            clock,
            new List<string>(),
            GlobalActionPlanEntryEngine.DefaultGlobalActionPlanEntryEngine(flags));
    }
}
