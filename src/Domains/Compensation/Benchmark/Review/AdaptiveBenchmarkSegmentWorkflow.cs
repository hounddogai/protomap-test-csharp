namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive benchmark segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveBenchmarkSegmentWorkflow(IAdaptiveBenchmarkSegmentStore Store, AdaptiveBenchmarkSegmentValidator Validator, IClock Clock, List<string> Journal)
{
    public static AdaptiveBenchmarkSegmentWorkflow NewAdaptiveBenchmarkSegmentWorkflow(IAdaptiveBenchmarkSegmentStore store, IClock clock)
    {
        return new AdaptiveBenchmarkSegmentWorkflow(
            store,
            AdaptiveBenchmarkSegmentRules.DefaultAdaptiveBenchmarkSegmentValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive benchmark segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveBenchmarkSegmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive benchmark segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveBenchmarkSegmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
            if (item.UpdatedAt > cutoff)
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
