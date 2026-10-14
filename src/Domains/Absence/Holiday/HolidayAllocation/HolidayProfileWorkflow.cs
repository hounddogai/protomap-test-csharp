namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves holiday profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayProfileManager(IHolidayProfileStore Store, HolidayProfileValidator Validator, IClock Clock, List<string> Journal, HolidayProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayProfileStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayProfileStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday profile {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of holiday profile workflow.</summary>
public static class HolidayProfileWorkflow
{
    public static HolidayProfileManager NewHolidayProfileManager(IHolidayProfileStore store, IClock clock)
    {
        return new HolidayProfileManager(
            store,
            HolidayProfileRules.DefaultHolidayProfileValidator(),
            clock,
            new List<string>(),
            HolidayProfileEngine.DefaultHolidayProfileEngine());
    }
}
