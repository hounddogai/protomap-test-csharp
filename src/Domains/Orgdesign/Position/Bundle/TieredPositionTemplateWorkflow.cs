namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered position templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredPositionTemplateCoordinator(ITieredPositionTemplateStore Store, TieredPositionTemplateValidator Validator, IClock Clock, List<string> Journal, TieredPositionTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered position template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredPositionTemplateStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered position template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredPositionTemplateStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered position template {code} not found");
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

/// <summary>Functions of tiered position template workflow.</summary>
public static class TieredPositionTemplateWorkflow
{
    public static TieredPositionTemplateCoordinator NewTieredPositionTemplateCoordinator(ITieredPositionTemplateStore store, IClock clock, Registry flags)
    {
        return new TieredPositionTemplateCoordinator(
            store,
            TieredPositionTemplateRules.DefaultTieredPositionTemplateValidator(),
            clock,
            new List<string>(),
            TieredPositionTemplateEngine.DefaultTieredPositionTemplateEngine(flags));
    }
}
