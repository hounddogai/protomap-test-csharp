namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves span rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanRollupWorkflow(ISpanRollupStore Store, SpanRollupValidator Validator, IClock Clock, List<string> Journal, SpanRollupEngine Engine)
{
    public static SpanRollupWorkflow NewSpanRollupWorkflow(ISpanRollupStore store, IClock clock, Registry flags)
    {
        return new SpanRollupWorkflow(
            store,
            SpanRollupRules.DefaultSpanRollupValidator(),
            clock,
            new List<string>(),
            SpanRollupEngine.DefaultSpanRollupEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanRollupStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanRollupStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span rollup {code} not found");
        }
        var result = Engine.Compute(item);
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
