namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy holiday bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyHolidayBundleCoordinator(ILegacyHolidayBundleStore Store, LegacyHolidayBundleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy holiday bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyHolidayBundleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy holiday bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyHolidayBundleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of legacy holiday bundle workflow.</summary>
public static class LegacyHolidayBundleWorkflow
{
    public static LegacyHolidayBundleCoordinator NewLegacyHolidayBundleCoordinator(ILegacyHolidayBundleStore store, IClock clock)
    {
        return new LegacyHolidayBundleCoordinator(
            store,
            LegacyHolidayBundleRules.DefaultLegacyHolidayBundleValidator(),
            clock,
            new List<string>());
    }
}
