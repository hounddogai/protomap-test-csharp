namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanAllocationDesk(ISpanAllocationStore Store, SpanAllocationValidator Validator, IClock Clock, List<string> Journal, SpanAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanAllocationStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanAllocationStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span allocation {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of span allocation workflow.</summary>
public static class SpanAllocationWorkflow
{
    public static SpanAllocationDesk NewSpanAllocationDesk(ISpanAllocationStore store, IClock clock)
    {
        return new SpanAllocationDesk(
            store,
            SpanAllocationRules.DefaultSpanAllocationValidator(),
            clock,
            new List<string>(),
            SpanAllocationEngine.DefaultSpanAllocationEngine());
    }
}
