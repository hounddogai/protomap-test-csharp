namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves span claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanClaimWorkflow(ISpanClaimStore Store, SpanClaimValidator Validator, IClock Clock, List<string> Journal, SpanClaimEngine Engine)
{
    public static SpanClaimWorkflow NewSpanClaimWorkflow(ISpanClaimStore store, IClock clock, Registry flags)
    {
        return new SpanClaimWorkflow(
            store,
            SpanClaimRules.DefaultSpanClaimValidator(),
            clock,
            new List<string>(),
            SpanClaimEngine.DefaultSpanClaimEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanClaimStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanClaimStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span claim {code} not found");
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
