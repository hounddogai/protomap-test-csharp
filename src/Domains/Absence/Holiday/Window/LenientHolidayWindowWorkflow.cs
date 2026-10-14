namespace Acme.Workforce.Absence.Holiday.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient holiday windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientHolidayWindowDesk(ILenientHolidayWindowStore Store, LenientHolidayWindowValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient holiday window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientHolidayWindowStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient holiday window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientHolidayWindowStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of lenient holiday window workflow.</summary>
public static class LenientHolidayWindowWorkflow
{
    public static LenientHolidayWindowDesk NewLenientHolidayWindowDesk(ILenientHolidayWindowStore store, IClock clock)
    {
        return new LenientHolidayWindowDesk(
            store,
            LenientHolidayWindowRules.DefaultLenientHolidayWindowValidator(),
            clock,
            new List<string>());
    }
}
