namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled desk summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledDeskSummaryCoordinator(IScheduledDeskSummaryStore Store, ScheduledDeskSummaryValidator Validator, IClock Clock, List<string> Journal, ScheduledDeskSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled desk summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledDeskSummaryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled desk summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledDeskSummaryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled desk summary {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of scheduled desk summary workflow.</summary>
public static class ScheduledDeskSummaryWorkflow
{
    public static ScheduledDeskSummaryCoordinator NewScheduledDeskSummaryCoordinator(IScheduledDeskSummaryStore store, IClock clock, Registry flags)
    {
        return new ScheduledDeskSummaryCoordinator(
            store,
            ScheduledDeskSummaryRules.DefaultScheduledDeskSummaryValidator(),
            clock,
            new List<string>(),
            ScheduledDeskSummaryEngine.DefaultScheduledDeskSummaryEngine(flags));
    }
}
