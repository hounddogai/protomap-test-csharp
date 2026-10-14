namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves refresh rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshRollupDesk(IRefreshRollupStore Store, RefreshRollupValidator Validator, IClock Clock, List<string> Journal, RefreshRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshRollupStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshRollupStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh rollup {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of refresh rollup workflow.</summary>
public static class RefreshRollupWorkflow
{
    public static RefreshRollupDesk NewRefreshRollupDesk(IRefreshRollupStore store, IClock clock, Registry flags)
    {
        return new RefreshRollupDesk(
            store,
            RefreshRollupRules.DefaultRefreshRollupValidator(),
            clock,
            new List<string>(),
            RefreshRollupEngine.DefaultRefreshRollupEngine(flags));
    }
}
