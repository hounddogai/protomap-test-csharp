namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered reorg variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredReorgVarianceManager(ITieredReorgVarianceStore Store, TieredReorgVarianceValidator Validator, IClock Clock, List<string> Journal, TieredReorgVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered reorg variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredReorgVarianceStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered reorg variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredReorgVarianceStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered reorg variance {code} not found");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of tiered reorg variance workflow.</summary>
public static class TieredReorgVarianceWorkflow
{
    public static TieredReorgVarianceManager NewTieredReorgVarianceManager(ITieredReorgVarianceStore store, IClock clock, Registry flags)
    {
        return new TieredReorgVarianceManager(
            store,
            TieredReorgVarianceRules.DefaultTieredReorgVarianceValidator(),
            clock,
            new List<string>(),
            TieredReorgVarianceEngine.DefaultTieredReorgVarianceEngine(flags));
    }
}
