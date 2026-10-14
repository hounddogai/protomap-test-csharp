namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled span digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledSpanDigestWorkflow(IScheduledSpanDigestStore Store, ScheduledSpanDigestValidator Validator, IClock Clock, List<string> Journal)
{
    public static ScheduledSpanDigestWorkflow NewScheduledSpanDigestWorkflow(IScheduledSpanDigestStore store, IClock clock)
    {
        return new ScheduledSpanDigestWorkflow(
            store,
            ScheduledSpanDigestRules.DefaultScheduledSpanDigestValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled span digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledSpanDigestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled span digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledSpanDigestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
