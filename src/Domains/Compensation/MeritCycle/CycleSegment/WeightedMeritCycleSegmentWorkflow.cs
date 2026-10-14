namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted merit cycle segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedMeritCycleSegmentWorkflow(IWeightedMeritCycleSegmentStore Store, WeightedMeritCycleSegmentValidator Validator, IClock Clock, List<string> Journal)
{
    public static WeightedMeritCycleSegmentWorkflow NewWeightedMeritCycleSegmentWorkflow(IWeightedMeritCycleSegmentStore store, IClock clock)
    {
        return new WeightedMeritCycleSegmentWorkflow(
            store,
            WeightedMeritCycleSegmentRules.DefaultWeightedMeritCycleSegmentValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted merit cycle segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedMeritCycleSegmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted merit cycle segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedMeritCycleSegmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
