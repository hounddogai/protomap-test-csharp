namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves reorg scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReorgScoreManager(IReorgScoreStore Store, ReorgScoreValidator Validator, IClock Clock, List<string> Journal, ReorgScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReorgScoreStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReorgScoreStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg score {code} not found");
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

/// <summary>Functions of reorg score workflow.</summary>
public static class ReorgScoreWorkflow
{
    public static ReorgScoreManager NewReorgScoreManager(IReorgScoreStore store, IClock clock, Registry flags)
    {
        return new ReorgScoreManager(
            store,
            ReorgScoreRules.DefaultReorgScoreValidator(),
            clock,
            new List<string>(),
            ReorgScoreEngine.DefaultReorgScoreEngine(flags));
    }
}
