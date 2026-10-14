namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves position mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionMappingWorkflow(IPositionMappingStore Store, PositionMappingValidator Validator, IClock Clock, List<string> Journal, PositionMappingEngine Engine)
{
    public static PositionMappingWorkflow NewPositionMappingWorkflow(IPositionMappingStore store, IClock clock, Registry flags)
    {
        return new PositionMappingWorkflow(
            store,
            PositionMappingRules.DefaultPositionMappingValidator(),
            clock,
            new List<string>(),
            PositionMappingEngine.DefaultPositionMappingEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionMappingStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionMappingStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position mapping {code} not found");
        }
        var result = Engine.Evaluate(item);
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
