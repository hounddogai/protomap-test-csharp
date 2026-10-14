namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback cost model rollups through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackCostModelRollupService(IFallbackCostModelRollupStore Store, FallbackCostModelRollupValidator Validator, IClock Clock, List<string> Journal, FallbackCostModelRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback cost model rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackCostModelRollupStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback cost model rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackCostModelRollupStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback cost model rollup {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of fallback cost model rollup workflow.</summary>
public static class FallbackCostModelRollupWorkflow
{
    public static FallbackCostModelRollupService NewFallbackCostModelRollupService(IFallbackCostModelRollupStore store, IClock clock, Registry flags)
    {
        return new FallbackCostModelRollupService(
            store,
            FallbackCostModelRollupRules.DefaultFallbackCostModelRollupValidator(),
            clock,
            new List<string>(),
            FallbackCostModelRollupEngine.DefaultFallbackCostModelRollupEngine(flags));
    }
}
