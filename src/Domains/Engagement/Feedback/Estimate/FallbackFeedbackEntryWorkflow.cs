namespace Acme.Workforce.Engagement.Feedback.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback feedback entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackFeedbackEntryCoordinator(IFallbackFeedbackEntryStore Store, FallbackFeedbackEntryValidator Validator, IClock Clock, List<string> Journal, FallbackFeedbackEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback feedback entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackFeedbackEntryStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback feedback entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackFeedbackEntryStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback feedback entry {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of fallback feedback entry workflow.</summary>
public static class FallbackFeedbackEntryWorkflow
{
    public static FallbackFeedbackEntryCoordinator NewFallbackFeedbackEntryCoordinator(IFallbackFeedbackEntryStore store, IClock clock)
    {
        return new FallbackFeedbackEntryCoordinator(
            store,
            FallbackFeedbackEntryRules.DefaultFallbackFeedbackEntryValidator(),
            clock,
            new List<string>(),
            FallbackFeedbackEntryEngine.DefaultFallbackFeedbackEntryEngine());
    }
}
