namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient timesheet plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientTimesheetPlanWorkflow(ILenientTimesheetPlanStore Store, LenientTimesheetPlanValidator Validator, IClock Clock, List<string> Journal, LenientTimesheetPlanEngine Engine)
{
    public static LenientTimesheetPlanWorkflow NewLenientTimesheetPlanWorkflow(ILenientTimesheetPlanStore store, IClock clock, Registry flags)
    {
        return new LenientTimesheetPlanWorkflow(
            store,
            LenientTimesheetPlanRules.DefaultLenientTimesheetPlanValidator(),
            clock,
            new List<string>(),
            LenientTimesheetPlanEngine.DefaultLenientTimesheetPlanEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient timesheet plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientTimesheetPlanStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient timesheet plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientTimesheetPlanStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient timesheet plan {code} not found");
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
