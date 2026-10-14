namespace Acme.Workforce.Orgdesign.Position.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionRollupService(IPositionRollupStore Store, PositionRollupValidator Validator, IClock Clock, List<string> Journal, PositionRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionRollupStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionRollupStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position rollup {code} not found");
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

/// <summary>Functions of position rollup workflow.</summary>
public static class PositionRollupWorkflow
{
    public static PositionRollupService NewPositionRollupService(IPositionRollupStore store, IClock clock)
    {
        return new PositionRollupService(
            store,
            PositionRollupRules.DefaultPositionRollupValidator(),
            clock,
            new List<string>(),
            PositionRollupEngine.DefaultPositionRollupEngine());
    }
}
