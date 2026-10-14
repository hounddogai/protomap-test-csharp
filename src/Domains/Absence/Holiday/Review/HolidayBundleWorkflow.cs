namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves holiday bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayBundleService(IHolidayBundleStore Store, HolidayBundleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayBundleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayBundleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of holiday bundle workflow.</summary>
public static class HolidayBundleWorkflow
{
    public static HolidayBundleService NewHolidayBundleService(IHolidayBundleStore store, IClock clock)
    {
        return new HolidayBundleService(
            store,
            HolidayBundleRules.DefaultHolidayBundleValidator(),
            clock,
            new List<string>());
    }
}
