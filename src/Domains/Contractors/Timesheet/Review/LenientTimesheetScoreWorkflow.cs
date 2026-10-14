namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient timesheet scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientTimesheetScoreWorkflow(ILenientTimesheetScoreStore Store, LenientTimesheetScoreValidator Validator, IClock Clock, List<string> Journal, LenientTimesheetScoreEngine Engine)
{
    public static LenientTimesheetScoreWorkflow NewLenientTimesheetScoreWorkflow(ILenientTimesheetScoreStore store, IClock clock)
    {
        return new LenientTimesheetScoreWorkflow(
            store,
            LenientTimesheetScoreRules.DefaultLenientTimesheetScoreValidator(),
            clock,
            new List<string>(),
            LenientTimesheetScoreEngine.DefaultLenientTimesheetScoreEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient timesheet score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientTimesheetScoreStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient timesheet score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientTimesheetScoreStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient timesheet score {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.UpdatedAt > cutoff)
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
