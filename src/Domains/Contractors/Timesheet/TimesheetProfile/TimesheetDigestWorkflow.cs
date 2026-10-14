namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves timesheet digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetDigestService(ITimesheetDigestStore Store, TimesheetDigestValidator Validator, IClock Clock, List<string> Journal, TimesheetDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetDigestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetDigestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet digest {code} not found");
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

/// <summary>Functions of timesheet digest workflow.</summary>
public static class TimesheetDigestWorkflow
{
    public static TimesheetDigestService NewTimesheetDigestService(ITimesheetDigestStore store, IClock clock)
    {
        return new TimesheetDigestService(
            store,
            TimesheetDigestRules.DefaultTimesheetDigestValidator(),
            clock,
            new List<string>(),
            TimesheetDigestEngine.DefaultTimesheetDigestEngine());
    }
}
