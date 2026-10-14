namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves equity grant rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record EquityGrantRollupCoordinator(IEquityGrantRollupStore Store, EquityGrantRollupValidator Validator, IClock Clock, List<string> Journal, EquityGrantRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EquityGrantRollupStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EquityGrantRollupStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant rollup {code} not found");
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

/// <summary>Functions of equity grant rollup workflow.</summary>
public static class EquityGrantRollupWorkflow
{
    public static EquityGrantRollupCoordinator NewEquityGrantRollupCoordinator(IEquityGrantRollupStore store, IClock clock, Registry flags)
    {
        return new EquityGrantRollupCoordinator(
            store,
            EquityGrantRollupRules.DefaultEquityGrantRollupValidator(),
            clock,
            new List<string>(),
            EquityGrantRollupEngine.DefaultEquityGrantRollupEngine(flags));
    }
}
