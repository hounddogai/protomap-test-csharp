namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves position metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionMetricService(IPositionMetricStore Store, PositionMetricValidator Validator, IClock Clock, List<string> Journal, PositionMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionMetricStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionMetricStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position metric {code} not found");
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

/// <summary>Functions of position metric workflow.</summary>
public static class PositionMetricWorkflow
{
    public static PositionMetricService NewPositionMetricService(IPositionMetricStore store, IClock clock, Registry flags)
    {
        return new PositionMetricService(
            store,
            PositionMetricRules.DefaultPositionMetricValidator(),
            clock,
            new List<string>(),
            PositionMetricEngine.DefaultPositionMetricEngine(flags));
    }
}
