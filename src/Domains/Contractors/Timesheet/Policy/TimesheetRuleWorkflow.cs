namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves timesheet rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetRuleWorkflow(ITimesheetRuleStore Store, TimesheetRuleValidator Validator, IClock Clock, List<string> Journal, TimesheetRuleEngine Engine)
{
    public static TimesheetRuleWorkflow NewTimesheetRuleWorkflow(ITimesheetRuleStore store, IClock clock)
    {
        return new TimesheetRuleWorkflow(
            store,
            TimesheetRuleRules.DefaultTimesheetRuleValidator(),
            clock,
            new List<string>(),
            TimesheetRuleEngine.DefaultTimesheetRuleEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetRuleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetRuleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet rule {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.StartsAt > cutoff)
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
