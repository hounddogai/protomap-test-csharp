namespace Acme.Workforce.Engagement.ActionPlan.PlanSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback action plan segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackActionPlanSegmentDesk(IFallbackActionPlanSegmentStore Store, FallbackActionPlanSegmentValidator Validator, IClock Clock, List<string> Journal, FallbackActionPlanSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback action plan segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackActionPlanSegmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback action plan segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackActionPlanSegmentStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback action plan segment {code} not found");
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

/// <summary>Functions of fallback action plan segment workflow.</summary>
public static class FallbackActionPlanSegmentWorkflow
{
    public static FallbackActionPlanSegmentDesk NewFallbackActionPlanSegmentDesk(IFallbackActionPlanSegmentStore store, IClock clock, Registry flags)
    {
        return new FallbackActionPlanSegmentDesk(
            store,
            FallbackActionPlanSegmentRules.DefaultFallbackActionPlanSegmentValidator(),
            clock,
            new List<string>(),
            FallbackActionPlanSegmentEngine.DefaultFallbackActionPlanSegmentEngine(flags));
    }
}
