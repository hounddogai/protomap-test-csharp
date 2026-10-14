namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves holiday assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayAssignmentCoordinator(IHolidayAssignmentStore Store, HolidayAssignmentValidator Validator, IClock Clock, List<string> Journal, HolidayAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayAssignmentStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayAssignmentStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday assignment {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of holiday assignment workflow.</summary>
public static class HolidayAssignmentWorkflow
{
    public static HolidayAssignmentCoordinator NewHolidayAssignmentCoordinator(IHolidayAssignmentStore store, IClock clock, Registry flags)
    {
        return new HolidayAssignmentCoordinator(
            store,
            HolidayAssignmentRules.DefaultHolidayAssignmentValidator(),
            clock,
            new List<string>(),
            HolidayAssignmentEngine.DefaultHolidayAssignmentEngine(flags));
    }
}
