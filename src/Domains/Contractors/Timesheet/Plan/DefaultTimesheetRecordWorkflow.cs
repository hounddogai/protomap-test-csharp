namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default timesheet records through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultTimesheetRecordWorkflow(IDefaultTimesheetRecordStore Store, DefaultTimesheetRecordValidator Validator, IClock Clock, List<string> Journal)
{
    public static DefaultTimesheetRecordWorkflow NewDefaultTimesheetRecordWorkflow(IDefaultTimesheetRecordStore store, IClock clock)
    {
        return new DefaultTimesheetRecordWorkflow(
            store,
            DefaultTimesheetRecordRules.DefaultDefaultTimesheetRecordValidator(),
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
            throw new InvalidOperationException($"default timesheet record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultTimesheetRecordStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default timesheet record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultTimesheetRecordStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
