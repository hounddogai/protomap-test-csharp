namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback training due segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackTrainingDueSegmentCoordinator(IFallbackTrainingDueSegmentStore Store, FallbackTrainingDueSegmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback training due segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackTrainingDueSegmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback training due segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackTrainingDueSegmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of fallback training due segment workflow.</summary>
public static class FallbackTrainingDueSegmentWorkflow
{
    public static FallbackTrainingDueSegmentCoordinator NewFallbackTrainingDueSegmentCoordinator(IFallbackTrainingDueSegmentStore store, IClock clock)
    {
        return new FallbackTrainingDueSegmentCoordinator(
            store,
            FallbackTrainingDueSegmentRules.DefaultFallbackTrainingDueSegmentValidator(),
            clock,
            new List<string>());
    }
}
