namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient span rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientSpanRollupManager(ILenientSpanRollupStore Store, LenientSpanRollupValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient span rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientSpanRollupStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient span rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientSpanRollupStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of lenient span rollup workflow.</summary>
public static class LenientSpanRollupWorkflow
{
    public static LenientSpanRollupManager NewLenientSpanRollupManager(ILenientSpanRollupStore store, IClock clock)
    {
        return new LenientSpanRollupManager(
            store,
            LenientSpanRollupRules.DefaultLenientSpanRollupValidator(),
            clock,
            new List<string>());
    }
}
