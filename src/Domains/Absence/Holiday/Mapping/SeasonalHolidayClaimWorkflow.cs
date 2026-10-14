namespace Acme.Workforce.Absence.Holiday.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal holiday claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalHolidayClaimWorkflow(ISeasonalHolidayClaimStore Store, SeasonalHolidayClaimValidator Validator, IClock Clock, List<string> Journal, SeasonalHolidayClaimEngine Engine)
{
    public static SeasonalHolidayClaimWorkflow NewSeasonalHolidayClaimWorkflow(ISeasonalHolidayClaimStore store, IClock clock)
    {
        return new SeasonalHolidayClaimWorkflow(
            store,
            SeasonalHolidayClaimRules.DefaultSeasonalHolidayClaimValidator(),
            clock,
            new List<string>(),
            SeasonalHolidayClaimEngine.DefaultSeasonalHolidayClaimEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal holiday claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalHolidayClaimStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal holiday claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalHolidayClaimStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal holiday claim {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.ExpiresAt > cutoff)
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
