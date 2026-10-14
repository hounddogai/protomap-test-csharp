namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional holiday windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalHolidayWindowCoordinator(IRegionalHolidayWindowStore Store, RegionalHolidayWindowValidator Validator, IClock Clock, List<string> Journal, RegionalHolidayWindowEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional holiday window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalHolidayWindowStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional holiday window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalHolidayWindowStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional holiday window {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of regional holiday window workflow.</summary>
public static class RegionalHolidayWindowWorkflow
{
    public static RegionalHolidayWindowCoordinator NewRegionalHolidayWindowCoordinator(IRegionalHolidayWindowStore store, IClock clock, Registry flags)
    {
        return new RegionalHolidayWindowCoordinator(
            store,
            RegionalHolidayWindowRules.DefaultRegionalHolidayWindowValidator(),
            clock,
            new List<string>(),
            RegionalHolidayWindowEngine.DefaultRegionalHolidayWindowEngine(flags));
    }
}
