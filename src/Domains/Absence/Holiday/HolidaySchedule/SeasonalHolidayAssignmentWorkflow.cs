namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal holiday assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalHolidayAssignmentManager(ISeasonalHolidayAssignmentStore Store, SeasonalHolidayAssignmentValidator Validator, IClock Clock, List<string> Journal, SeasonalHolidayAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal holiday assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalHolidayAssignmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal holiday assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalHolidayAssignmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal holiday assignment {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of seasonal holiday assignment workflow.</summary>
public static class SeasonalHolidayAssignmentWorkflow
{
    public static SeasonalHolidayAssignmentManager NewSeasonalHolidayAssignmentManager(ISeasonalHolidayAssignmentStore store, IClock clock, Registry flags)
    {
        return new SeasonalHolidayAssignmentManager(
            store,
            SeasonalHolidayAssignmentRules.DefaultSeasonalHolidayAssignmentValidator(),
            clock,
            new List<string>(),
            SeasonalHolidayAssignmentEngine.DefaultSeasonalHolidayAssignmentEngine(flags));
    }
}
