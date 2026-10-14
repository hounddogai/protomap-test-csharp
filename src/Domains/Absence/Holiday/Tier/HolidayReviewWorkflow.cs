namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves holiday reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayReviewManager(IHolidayReviewStore Store, HolidayReviewValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayReviewStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayReviewStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of holiday review workflow.</summary>
public static class HolidayReviewWorkflow
{
    public static HolidayReviewManager NewHolidayReviewManager(IHolidayReviewStore store, IClock clock)
    {
        return new HolidayReviewManager(
            store,
            HolidayReviewRules.DefaultHolidayReviewValidator(),
            clock,
            new List<string>());
    }
}
