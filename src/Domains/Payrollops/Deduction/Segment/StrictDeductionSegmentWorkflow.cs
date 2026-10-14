namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict deduction segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictDeductionSegmentManager(IStrictDeductionSegmentStore Store, StrictDeductionSegmentValidator Validator, IClock Clock, List<string> Journal, StrictDeductionSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict deduction segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictDeductionSegmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict deduction segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictDeductionSegmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict deduction segment {code} not found");
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

/// <summary>Functions of strict deduction segment workflow.</summary>
public static class StrictDeductionSegmentWorkflow
{
    public static StrictDeductionSegmentManager NewStrictDeductionSegmentManager(IStrictDeductionSegmentStore store, IClock clock, Registry flags)
    {
        return new StrictDeductionSegmentManager(
            store,
            StrictDeductionSegmentRules.DefaultStrictDeductionSegmentValidator(),
            clock,
            new List<string>(),
            StrictDeductionSegmentEngine.DefaultStrictDeductionSegmentEngine(flags));
    }
}
