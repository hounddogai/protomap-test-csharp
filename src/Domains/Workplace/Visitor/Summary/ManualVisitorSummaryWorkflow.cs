namespace Acme.Workforce.Workplace.Visitor.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual visitor summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualVisitorSummaryService(IManualVisitorSummaryStore Store, ManualVisitorSummaryValidator Validator, IClock Clock, List<string> Journal, ManualVisitorSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual visitor summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualVisitorSummaryStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual visitor summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualVisitorSummaryStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual visitor summary {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of manual visitor summary workflow.</summary>
public static class ManualVisitorSummaryWorkflow
{
    public static ManualVisitorSummaryService NewManualVisitorSummaryService(IManualVisitorSummaryStore store, IClock clock, Registry flags)
    {
        return new ManualVisitorSummaryService(
            store,
            ManualVisitorSummaryRules.DefaultManualVisitorSummaryValidator(),
            clock,
            new List<string>(),
            ManualVisitorSummaryEngine.DefaultManualVisitorSummaryEngine(flags));
    }
}
