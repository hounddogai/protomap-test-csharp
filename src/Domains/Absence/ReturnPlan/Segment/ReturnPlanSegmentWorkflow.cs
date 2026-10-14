namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves return plan segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReturnPlanSegmentCoordinator(IReturnPlanSegmentStore Store, ReturnPlanSegmentValidator Validator, IClock Clock, List<string> Journal, ReturnPlanSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReturnPlanSegmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReturnPlanSegmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan segment {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of return plan segment workflow.</summary>
public static class ReturnPlanSegmentWorkflow
{
    public static ReturnPlanSegmentCoordinator NewReturnPlanSegmentCoordinator(IReturnPlanSegmentStore store, IClock clock)
    {
        return new ReturnPlanSegmentCoordinator(
            store,
            ReturnPlanSegmentRules.DefaultReturnPlanSegmentValidator(),
            clock,
            new List<string>(),
            ReturnPlanSegmentEngine.DefaultReturnPlanSegmentEngine());
    }
}
