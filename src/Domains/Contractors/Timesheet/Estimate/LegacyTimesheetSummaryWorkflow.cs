namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy timesheet summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyTimesheetSummaryWorkflow(ILegacyTimesheetSummaryStore Store, LegacyTimesheetSummaryValidator Validator, IClock Clock, List<string> Journal, LegacyTimesheetSummaryEngine Engine)
{
    public static LegacyTimesheetSummaryWorkflow NewLegacyTimesheetSummaryWorkflow(ILegacyTimesheetSummaryStore store, IClock clock, Registry flags)
    {
        return new LegacyTimesheetSummaryWorkflow(
            store,
            LegacyTimesheetSummaryRules.DefaultLegacyTimesheetSummaryValidator(),
            clock,
            new List<string>(),
            LegacyTimesheetSummaryEngine.DefaultLegacyTimesheetSummaryEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy timesheet summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyTimesheetSummaryStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy timesheet summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyTimesheetSummaryStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy timesheet summary {code} not found");
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
