namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal nomination cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalNominationCycleWorkflow(ISeasonalNominationCycleStore Store, SeasonalNominationCycleValidator Validator, IClock Clock, List<string> Journal)
{
    public static SeasonalNominationCycleWorkflow NewSeasonalNominationCycleWorkflow(ISeasonalNominationCycleStore store, IClock clock)
    {
        return new SeasonalNominationCycleWorkflow(
            store,
            SeasonalNominationCycleRules.DefaultSeasonalNominationCycleValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal nomination cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalNominationCycleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal nomination cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalNominationCycleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public int Sweep(int cutoff)
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
