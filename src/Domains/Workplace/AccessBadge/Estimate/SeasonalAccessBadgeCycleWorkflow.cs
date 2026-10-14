namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal access badge cycles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalAccessBadgeCycleWorkflow(ISeasonalAccessBadgeCycleStore Store, SeasonalAccessBadgeCycleValidator Validator, IClock Clock, List<string> Journal)
{
    public static SeasonalAccessBadgeCycleWorkflow NewSeasonalAccessBadgeCycleWorkflow(ISeasonalAccessBadgeCycleStore store, IClock clock)
    {
        return new SeasonalAccessBadgeCycleWorkflow(
            store,
            SeasonalAccessBadgeCycleRules.DefaultSeasonalAccessBadgeCycleValidator(),
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
            throw new InvalidOperationException($"seasonal access badge cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalAccessBadgeCycleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal access badge cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalAccessBadgeCycleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
