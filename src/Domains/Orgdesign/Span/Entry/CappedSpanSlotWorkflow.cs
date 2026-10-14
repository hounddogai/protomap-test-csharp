namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped span slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedSpanSlotService(ICappedSpanSlotStore Store, CappedSpanSlotValidator Validator, IClock Clock, List<string> Journal, CappedSpanSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped span slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedSpanSlotStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped span slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedSpanSlotStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped span slot {code} not found");
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

/// <summary>Functions of capped span slot workflow.</summary>
public static class CappedSpanSlotWorkflow
{
    public static CappedSpanSlotService NewCappedSpanSlotService(ICappedSpanSlotStore store, IClock clock, Registry flags)
    {
        return new CappedSpanSlotService(
            store,
            CappedSpanSlotRules.DefaultCappedSpanSlotValidator(),
            clock,
            new List<string>(),
            CappedSpanSlotEngine.DefaultCappedSpanSlotEngine(flags));
    }
}
