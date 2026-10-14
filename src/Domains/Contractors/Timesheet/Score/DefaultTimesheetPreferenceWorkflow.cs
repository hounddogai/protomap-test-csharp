namespace Acme.Workforce.Contractors.Timesheet.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default timesheet preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultTimesheetPreferenceWorkflow(IDefaultTimesheetPreferenceStore Store, DefaultTimesheetPreferenceValidator Validator, IClock Clock, List<string> Journal, DefaultTimesheetPreferenceEngine Engine)
{
    public static DefaultTimesheetPreferenceWorkflow NewDefaultTimesheetPreferenceWorkflow(IDefaultTimesheetPreferenceStore store, IClock clock, Registry flags)
    {
        return new DefaultTimesheetPreferenceWorkflow(
            store,
            DefaultTimesheetPreferenceRules.DefaultDefaultTimesheetPreferenceValidator(),
            clock,
            new List<string>(),
            DefaultTimesheetPreferenceEngine.DefaultDefaultTimesheetPreferenceEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default timesheet preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultTimesheetPreferenceStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default timesheet preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultTimesheetPreferenceStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default timesheet preference {code} not found");
        }
        var result = Engine.Run(item);
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
