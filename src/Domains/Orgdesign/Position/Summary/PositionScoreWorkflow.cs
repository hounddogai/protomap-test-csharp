namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves position scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionScoreManager(IPositionScoreStore Store, PositionScoreValidator Validator, IClock Clock, List<string> Journal, PositionScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionScoreStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionScoreStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position score {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of position score workflow.</summary>
public static class PositionScoreWorkflow
{
    public static PositionScoreManager NewPositionScoreManager(IPositionScoreStore store, IClock clock, Registry flags)
    {
        return new PositionScoreManager(
            store,
            PositionScoreRules.DefaultPositionScoreValidator(),
            clock,
            new List<string>(),
            PositionScoreEngine.DefaultPositionScoreEngine(flags));
    }
}
