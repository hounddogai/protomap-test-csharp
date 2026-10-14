namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted balance thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedBalanceThresholdService(IWeightedBalanceThresholdStore Store, WeightedBalanceThresholdValidator Validator, IClock Clock, List<string> Journal, WeightedBalanceThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted balance threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedBalanceThresholdStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted balance threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedBalanceThresholdStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted balance threshold {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of weighted balance threshold workflow.</summary>
public static class WeightedBalanceThresholdWorkflow
{
    public static WeightedBalanceThresholdService NewWeightedBalanceThresholdService(IWeightedBalanceThresholdStore store, IClock clock, Registry flags)
    {
        return new WeightedBalanceThresholdService(
            store,
            WeightedBalanceThresholdRules.DefaultWeightedBalanceThresholdValidator(),
            clock,
            new List<string>(),
            WeightedBalanceThresholdEngine.DefaultWeightedBalanceThresholdEngine(flags));
    }
}
