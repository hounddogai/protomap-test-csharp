namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling refresh rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingRefreshRollupService(IRollingRefreshRollupStore Store, RollingRefreshRollupValidator Validator, IClock Clock, List<string> Journal, RollingRefreshRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling refresh rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingRefreshRollupStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling refresh rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingRefreshRollupStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling refresh rollup {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of rolling refresh rollup workflow.</summary>
public static class RollingRefreshRollupWorkflow
{
    public static RollingRefreshRollupService NewRollingRefreshRollupService(IRollingRefreshRollupStore store, IClock clock)
    {
        return new RollingRefreshRollupService(
            store,
            RollingRefreshRollupRules.DefaultRollingRefreshRollupValidator(),
            clock,
            new List<string>(),
            RollingRefreshRollupEngine.DefaultRollingRefreshRollupEngine());
    }
}
