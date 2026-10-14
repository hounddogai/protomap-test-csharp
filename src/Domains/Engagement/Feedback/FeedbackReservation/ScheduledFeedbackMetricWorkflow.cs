namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled feedback metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledFeedbackMetricWorkflow(IScheduledFeedbackMetricStore Store, ScheduledFeedbackMetricValidator Validator, IClock Clock, List<string> Journal, ScheduledFeedbackMetricEngine Engine)
{
    public static ScheduledFeedbackMetricWorkflow NewScheduledFeedbackMetricWorkflow(IScheduledFeedbackMetricStore store, IClock clock, Registry flags)
    {
        return new ScheduledFeedbackMetricWorkflow(
            store,
            ScheduledFeedbackMetricRules.DefaultScheduledFeedbackMetricValidator(),
            clock,
            new List<string>(),
            ScheduledFeedbackMetricEngine.DefaultScheduledFeedbackMetricEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled feedback metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledFeedbackMetricStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled feedback metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledFeedbackMetricStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled feedback metric {code} not found");
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
