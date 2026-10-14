namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves position bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionBundleService(IPositionBundleStore Store, PositionBundleValidator Validator, IClock Clock, List<string> Journal, PositionBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionBundleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionBundleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position bundle {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of position bundle workflow.</summary>
public static class PositionBundleWorkflow
{
    public static PositionBundleService NewPositionBundleService(IPositionBundleStore store, IClock clock, Registry flags)
    {
        return new PositionBundleService(
            store,
            PositionBundleRules.DefaultPositionBundleValidator(),
            clock,
            new List<string>(),
            PositionBundleEngine.DefaultPositionBundleEngine(flags));
    }
}
