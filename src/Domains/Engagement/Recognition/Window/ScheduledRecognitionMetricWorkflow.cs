namespace Acme.Workforce.Engagement.Recognition.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled recognition metrics through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledRecognitionMetricDesk(IScheduledRecognitionMetricStore Store, ScheduledRecognitionMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled recognition metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledRecognitionMetricStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled recognition metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledRecognitionMetricStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of scheduled recognition metric workflow.</summary>
public static class ScheduledRecognitionMetricWorkflow
{
    public static ScheduledRecognitionMetricDesk NewScheduledRecognitionMetricDesk(IScheduledRecognitionMetricStore store, IClock clock)
    {
        return new ScheduledRecognitionMetricDesk(
            store,
            ScheduledRecognitionMetricRules.DefaultScheduledRecognitionMetricValidator(),
            clock,
            new List<string>());
    }
}
