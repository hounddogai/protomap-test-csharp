namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted headcount rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedHeadcountRollupService(IWeightedHeadcountRollupStore Store, WeightedHeadcountRollupValidator Validator, IClock Clock, List<string> Journal, WeightedHeadcountRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted headcount rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedHeadcountRollupStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted headcount rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedHeadcountRollupStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted headcount rollup {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of weighted headcount rollup workflow.</summary>
public static class WeightedHeadcountRollupWorkflow
{
    public static WeightedHeadcountRollupService NewWeightedHeadcountRollupService(IWeightedHeadcountRollupStore store, IClock clock, Registry flags)
    {
        return new WeightedHeadcountRollupService(
            store,
            WeightedHeadcountRollupRules.DefaultWeightedHeadcountRollupValidator(),
            clock,
            new List<string>(),
            WeightedHeadcountRollupEngine.DefaultWeightedHeadcountRollupEngine(flags));
    }
}
