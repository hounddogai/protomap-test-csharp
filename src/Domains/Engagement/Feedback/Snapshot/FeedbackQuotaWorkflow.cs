namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves feedback quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record FeedbackQuotaManager(IFeedbackQuotaStore Store, FeedbackQuotaValidator Validator, IClock Clock, List<string> Journal, FeedbackQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FeedbackQuotaStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FeedbackQuotaStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback quota {code} not found");
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

/// <summary>Functions of feedback quota workflow.</summary>
public static class FeedbackQuotaWorkflow
{
    public static FeedbackQuotaManager NewFeedbackQuotaManager(IFeedbackQuotaStore store, IClock clock, Registry flags)
    {
        return new FeedbackQuotaManager(
            store,
            FeedbackQuotaRules.DefaultFeedbackQuotaValidator(),
            clock,
            new List<string>(),
            FeedbackQuotaEngine.DefaultFeedbackQuotaEngine(flags));
    }
}
