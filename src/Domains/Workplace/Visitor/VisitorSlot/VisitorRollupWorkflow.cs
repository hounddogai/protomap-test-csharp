namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves visitor rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorRollupCoordinator(IVisitorRollupStore Store, VisitorRollupValidator Validator, IClock Clock, List<string> Journal, VisitorRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorRollupStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorRollupStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor rollup {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of visitor rollup workflow.</summary>
public static class VisitorRollupWorkflow
{
    public static VisitorRollupCoordinator NewVisitorRollupCoordinator(IVisitorRollupStore store, IClock clock, Registry flags)
    {
        return new VisitorRollupCoordinator(
            store,
            VisitorRollupRules.DefaultVisitorRollupValidator(),
            clock,
            new List<string>(),
            VisitorRollupEngine.DefaultVisitorRollupEngine(flags));
    }
}
