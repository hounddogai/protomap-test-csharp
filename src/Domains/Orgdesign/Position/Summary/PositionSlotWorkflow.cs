namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves position slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionSlotWorkflow(IPositionSlotStore Store, PositionSlotValidator Validator, IClock Clock, List<string> Journal, PositionSlotEngine Engine)
{
    public static PositionSlotWorkflow NewPositionSlotWorkflow(IPositionSlotStore store, IClock clock, Registry flags)
    {
        return new PositionSlotWorkflow(
            store,
            PositionSlotRules.DefaultPositionSlotValidator(),
            clock,
            new List<string>(),
            PositionSlotEngine.DefaultPositionSlotEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionSlotStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionSlotStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position slot {code} not found");
        }
        var result = Engine.Evaluate(item);
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
