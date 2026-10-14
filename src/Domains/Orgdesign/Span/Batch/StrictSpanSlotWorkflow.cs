namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict span slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictSpanSlotManager(IStrictSpanSlotStore Store, StrictSpanSlotValidator Validator, IClock Clock, List<string> Journal, StrictSpanSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict span slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictSpanSlotStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict span slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictSpanSlotStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict span slot {code} not found");
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

/// <summary>Functions of strict span slot workflow.</summary>
public static class StrictSpanSlotWorkflow
{
    public static StrictSpanSlotManager NewStrictSpanSlotManager(IStrictSpanSlotStore store, IClock clock)
    {
        return new StrictSpanSlotManager(
            store,
            StrictSpanSlotRules.DefaultStrictSpanSlotValidator(),
            clock,
            new List<string>(),
            StrictSpanSlotEngine.DefaultStrictSpanSlotEngine());
    }
}
