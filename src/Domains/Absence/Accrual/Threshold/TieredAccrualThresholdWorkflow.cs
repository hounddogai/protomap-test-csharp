namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered accrual thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredAccrualThresholdWorkflow(ITieredAccrualThresholdStore Store, TieredAccrualThresholdValidator Validator, IClock Clock, List<string> Journal, TieredAccrualThresholdEngine Engine)
{
    public static TieredAccrualThresholdWorkflow NewTieredAccrualThresholdWorkflow(ITieredAccrualThresholdStore store, IClock clock, Registry flags)
    {
        return new TieredAccrualThresholdWorkflow(
            store,
            TieredAccrualThresholdRules.DefaultTieredAccrualThresholdValidator(),
            clock,
            new List<string>(),
            TieredAccrualThresholdEngine.DefaultTieredAccrualThresholdEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered accrual threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredAccrualThresholdStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered accrual threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredAccrualThresholdStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered accrual threshold {code} not found");
        }
        var result = Engine.Evaluate(item);
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
