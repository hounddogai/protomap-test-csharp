namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary access badge summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryAccessBadgeSummaryManager(IPrimaryAccessBadgeSummaryStore Store, PrimaryAccessBadgeSummaryValidator Validator, IClock Clock, List<string> Journal, PrimaryAccessBadgeSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary access badge summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryAccessBadgeSummaryStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary access badge summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryAccessBadgeSummaryStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary access badge summary {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of primary access badge summary workflow.</summary>
public static class PrimaryAccessBadgeSummaryWorkflow
{
    public static PrimaryAccessBadgeSummaryManager NewPrimaryAccessBadgeSummaryManager(IPrimaryAccessBadgeSummaryStore store, IClock clock, Registry flags)
    {
        return new PrimaryAccessBadgeSummaryManager(
            store,
            PrimaryAccessBadgeSummaryRules.DefaultPrimaryAccessBadgeSummaryValidator(),
            clock,
            new List<string>(),
            PrimaryAccessBadgeSummaryEngine.DefaultPrimaryAccessBadgeSummaryEngine(flags));
    }
}
