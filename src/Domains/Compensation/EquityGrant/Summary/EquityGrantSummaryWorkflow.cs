namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves equity grant summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record EquityGrantSummaryManager(IEquityGrantSummaryStore Store, EquityGrantSummaryValidator Validator, IClock Clock, List<string> Journal, EquityGrantSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EquityGrantSummaryStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EquityGrantSummaryStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant summary {code} not found");
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

/// <summary>Functions of equity grant summary workflow.</summary>
public static class EquityGrantSummaryWorkflow
{
    public static EquityGrantSummaryManager NewEquityGrantSummaryManager(IEquityGrantSummaryStore store, IClock clock, Registry flags)
    {
        return new EquityGrantSummaryManager(
            store,
            EquityGrantSummaryRules.DefaultEquityGrantSummaryValidator(),
            clock,
            new List<string>(),
            EquityGrantSummaryEngine.DefaultEquityGrantSummaryEngine(flags));
    }
}
