namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal cost model preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalCostModelPreferenceService(ISeasonalCostModelPreferenceStore Store, SeasonalCostModelPreferenceValidator Validator, IClock Clock, List<string> Journal, SeasonalCostModelPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal cost model preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalCostModelPreferenceStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal cost model preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalCostModelPreferenceStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal cost model preference {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of seasonal cost model preference workflow.</summary>
public static class SeasonalCostModelPreferenceWorkflow
{
    public static SeasonalCostModelPreferenceService NewSeasonalCostModelPreferenceService(ISeasonalCostModelPreferenceStore store, IClock clock, Registry flags)
    {
        return new SeasonalCostModelPreferenceService(
            store,
            SeasonalCostModelPreferenceRules.DefaultSeasonalCostModelPreferenceValidator(),
            clock,
            new List<string>(),
            SeasonalCostModelPreferenceEngine.DefaultSeasonalCostModelPreferenceEngine(flags));
    }
}
