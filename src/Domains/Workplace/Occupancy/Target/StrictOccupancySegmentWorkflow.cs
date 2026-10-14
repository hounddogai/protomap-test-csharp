namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict occupancy segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictOccupancySegmentWorkflow(IStrictOccupancySegmentStore Store, StrictOccupancySegmentValidator Validator, IClock Clock, List<string> Journal, StrictOccupancySegmentEngine Engine)
{
    public static StrictOccupancySegmentWorkflow NewStrictOccupancySegmentWorkflow(IStrictOccupancySegmentStore store, IClock clock, Registry flags)
    {
        return new StrictOccupancySegmentWorkflow(
            store,
            StrictOccupancySegmentRules.DefaultStrictOccupancySegmentValidator(),
            clock,
            new List<string>(),
            StrictOccupancySegmentEngine.DefaultStrictOccupancySegmentEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict occupancy segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictOccupancySegmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict occupancy segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictOccupancySegmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict occupancy segment {code} not found");
        }
        var result = Engine.Compute(item);
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
