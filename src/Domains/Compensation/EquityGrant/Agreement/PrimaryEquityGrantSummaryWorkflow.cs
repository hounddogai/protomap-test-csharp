namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary equity grant summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryEquityGrantSummaryDesk(IPrimaryEquityGrantSummaryStore Store, PrimaryEquityGrantSummaryValidator Validator, IClock Clock, List<string> Journal, PrimaryEquityGrantSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary equity grant summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryEquityGrantSummaryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary equity grant summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryEquityGrantSummaryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary equity grant summary {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of primary equity grant summary workflow.</summary>
public static class PrimaryEquityGrantSummaryWorkflow
{
    public static PrimaryEquityGrantSummaryDesk NewPrimaryEquityGrantSummaryDesk(IPrimaryEquityGrantSummaryStore store, IClock clock, Registry flags)
    {
        return new PrimaryEquityGrantSummaryDesk(
            store,
            PrimaryEquityGrantSummaryRules.DefaultPrimaryEquityGrantSummaryValidator(),
            clock,
            new List<string>(),
            PrimaryEquityGrantSummaryEngine.DefaultPrimaryEquityGrantSummaryEngine(flags));
    }
}
