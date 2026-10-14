namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves feedback thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record FeedbackThresholdWorkflow(IFeedbackThresholdStore Store, FeedbackThresholdValidator Validator, IClock Clock, List<string> Journal, FeedbackThresholdEngine Engine)
{
    public static FeedbackThresholdWorkflow NewFeedbackThresholdWorkflow(IFeedbackThresholdStore store, IClock clock, Registry flags)
    {
        return new FeedbackThresholdWorkflow(
            store,
            FeedbackThresholdRules.DefaultFeedbackThresholdValidator(),
            clock,
            new List<string>(),
            FeedbackThresholdEngine.DefaultFeedbackThresholdEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FeedbackThresholdStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FeedbackThresholdStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback threshold {code} not found");
        }
        var result = Engine.Evaluate(item);
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
