namespace Acme.Workforce.Orgdesign.Span.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves span digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanDigestManager(ISpanDigestStore Store, SpanDigestValidator Validator, IClock Clock, List<string> Journal, SpanDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanDigestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanDigestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span digest {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of span digest workflow.</summary>
public static class SpanDigestWorkflow
{
    public static SpanDigestManager NewSpanDigestManager(ISpanDigestStore store, IClock clock, Registry flags)
    {
        return new SpanDigestManager(
            store,
            SpanDigestRules.DefaultSpanDigestValidator(),
            clock,
            new List<string>(),
            SpanDigestEngine.DefaultSpanDigestEngine(flags));
    }
}
