namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global action plan segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalActionPlanSegmentService(IGlobalActionPlanSegmentStore Store, GlobalActionPlanSegmentValidator Validator, IClock Clock, List<string> Journal, GlobalActionPlanSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global action plan segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalActionPlanSegmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global action plan segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalActionPlanSegmentStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global action plan segment {code} not found");
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

/// <summary>Functions of global action plan segment workflow.</summary>
public static class GlobalActionPlanSegmentWorkflow
{
    public static GlobalActionPlanSegmentService NewGlobalActionPlanSegmentService(IGlobalActionPlanSegmentStore store, IClock clock, Registry flags)
    {
        return new GlobalActionPlanSegmentService(
            store,
            GlobalActionPlanSegmentRules.DefaultGlobalActionPlanSegmentValidator(),
            clock,
            new List<string>(),
            GlobalActionPlanSegmentEngine.DefaultGlobalActionPlanSegmentEngine(flags));
    }
}
