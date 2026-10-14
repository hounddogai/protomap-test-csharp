namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback holiday schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackHolidayScheduleWorkflow(IFallbackHolidayScheduleStore Store, FallbackHolidayScheduleValidator Validator, IClock Clock, List<string> Journal, FallbackHolidayScheduleEngine Engine)
{
    public static FallbackHolidayScheduleWorkflow NewFallbackHolidayScheduleWorkflow(IFallbackHolidayScheduleStore store, IClock clock, Registry flags)
    {
        return new FallbackHolidayScheduleWorkflow(
            store,
            FallbackHolidayScheduleRules.DefaultFallbackHolidayScheduleValidator(),
            clock,
            new List<string>(),
            FallbackHolidayScheduleEngine.DefaultFallbackHolidayScheduleEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback holiday schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackHolidayScheduleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback holiday schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackHolidayScheduleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback holiday schedule {code} not found");
        }
        var result = Engine.Price(item);
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
