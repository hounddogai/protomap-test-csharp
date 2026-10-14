namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy timesheet estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyTimesheetEstimateWorkflow(ILegacyTimesheetEstimateStore Store, LegacyTimesheetEstimateValidator Validator, IClock Clock, List<string> Journal, LegacyTimesheetEstimateEngine Engine)
{
    public static LegacyTimesheetEstimateWorkflow NewLegacyTimesheetEstimateWorkflow(ILegacyTimesheetEstimateStore store, IClock clock, Registry flags)
    {
        return new LegacyTimesheetEstimateWorkflow(
            store,
            LegacyTimesheetEstimateRules.DefaultLegacyTimesheetEstimateValidator(),
            clock,
            new List<string>(),
            LegacyTimesheetEstimateEngine.DefaultLegacyTimesheetEstimateEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy timesheet estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyTimesheetEstimateStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy timesheet estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyTimesheetEstimateStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy timesheet estimate {code} not found");
        }
        var result = Engine.Compute(item);
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
