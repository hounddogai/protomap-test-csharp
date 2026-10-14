namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled mobility requests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledMobilityRequestService(IScheduledMobilityRequestStore Store, ScheduledMobilityRequestValidator Validator, IClock Clock, List<string> Journal, ScheduledMobilityRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled mobility request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledMobilityRequestStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled mobility request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledMobilityRequestStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled mobility request {code} not found");
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

/// <summary>Functions of scheduled mobility request workflow.</summary>
public static class ScheduledMobilityRequestWorkflow
{
    public static ScheduledMobilityRequestService NewScheduledMobilityRequestService(IScheduledMobilityRequestStore store, IClock clock, Registry flags)
    {
        return new ScheduledMobilityRequestService(
            store,
            ScheduledMobilityRequestRules.DefaultScheduledMobilityRequestValidator(),
            clock,
            new List<string>(),
            ScheduledMobilityRequestEngine.DefaultScheduledMobilityRequestEngine(flags));
    }
}
