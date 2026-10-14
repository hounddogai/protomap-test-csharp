namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled booking templates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledBookingTemplateManager(IScheduledBookingTemplateStore Store, ScheduledBookingTemplateValidator Validator, IClock Clock, List<string> Journal, ScheduledBookingTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled booking template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledBookingTemplateStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled booking template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledBookingTemplateStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled booking template {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of scheduled booking template workflow.</summary>
public static class ScheduledBookingTemplateWorkflow
{
    public static ScheduledBookingTemplateManager NewScheduledBookingTemplateManager(IScheduledBookingTemplateStore store, IClock clock, Registry flags)
    {
        return new ScheduledBookingTemplateManager(
            store,
            ScheduledBookingTemplateRules.DefaultScheduledBookingTemplateValidator(),
            clock,
            new List<string>(),
            ScheduledBookingTemplateEngine.DefaultScheduledBookingTemplateEngine(flags));
    }
}
