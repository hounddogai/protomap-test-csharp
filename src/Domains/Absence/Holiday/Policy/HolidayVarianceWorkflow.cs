namespace Acme.Workforce.Absence.Holiday.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves holiday variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayVarianceDesk(IHolidayVarianceStore Store, HolidayVarianceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayVarianceStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayVarianceStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of holiday variance workflow.</summary>
public static class HolidayVarianceWorkflow
{
    public static HolidayVarianceDesk NewHolidayVarianceDesk(IHolidayVarianceStore store, IClock clock)
    {
        return new HolidayVarianceDesk(
            store,
            HolidayVarianceRules.DefaultHolidayVarianceValidator(),
            clock,
            new List<string>());
    }
}
