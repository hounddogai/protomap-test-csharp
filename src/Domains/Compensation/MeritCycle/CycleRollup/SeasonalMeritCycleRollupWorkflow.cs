namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal merit cycle rollups through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalMeritCycleRollupWorkflow(ISeasonalMeritCycleRollupStore Store, SeasonalMeritCycleRollupValidator Validator, IClock Clock, List<string> Journal, SeasonalMeritCycleRollupEngine Engine)
{
    public static SeasonalMeritCycleRollupWorkflow NewSeasonalMeritCycleRollupWorkflow(ISeasonalMeritCycleRollupStore store, IClock clock, Registry flags)
    {
        return new SeasonalMeritCycleRollupWorkflow(
            store,
            SeasonalMeritCycleRollupRules.DefaultSeasonalMeritCycleRollupValidator(),
            clock,
            new List<string>(),
            SeasonalMeritCycleRollupEngine.DefaultSeasonalMeritCycleRollupEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal merit cycle rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalMeritCycleRollupStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal merit cycle rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalMeritCycleRollupStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal merit cycle rollup {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.EndsAt > cutoff)
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
