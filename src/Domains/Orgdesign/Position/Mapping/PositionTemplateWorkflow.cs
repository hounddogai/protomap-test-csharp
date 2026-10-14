namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves position templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionTemplateCoordinator(IPositionTemplateStore Store, PositionTemplateValidator Validator, IClock Clock, List<string> Journal, PositionTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionTemplateStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionTemplateStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position template {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of position template workflow.</summary>
public static class PositionTemplateWorkflow
{
    public static PositionTemplateCoordinator NewPositionTemplateCoordinator(IPositionTemplateStore store, IClock clock, Registry flags)
    {
        return new PositionTemplateCoordinator(
            store,
            PositionTemplateRules.DefaultPositionTemplateValidator(),
            clock,
            new List<string>(),
            PositionTemplateEngine.DefaultPositionTemplateEngine(flags));
    }
}
