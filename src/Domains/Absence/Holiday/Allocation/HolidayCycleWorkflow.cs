namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves holiday cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayCycleDesk(IHolidayCycleStore Store, HolidayCycleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayCycleStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayCycleStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of holiday cycle workflow.</summary>
public static class HolidayCycleWorkflow
{
    public static HolidayCycleDesk NewHolidayCycleDesk(IHolidayCycleStore store, IClock clock)
    {
        return new HolidayCycleDesk(
            store,
            HolidayCycleRules.DefaultHolidayCycleValidator(),
            clock,
            new List<string>());
    }
}
