namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive balance summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveBalanceSummaryService(IAdaptiveBalanceSummaryStore Store, AdaptiveBalanceSummaryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive balance summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveBalanceSummaryStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive balance summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveBalanceSummaryStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of adaptive balance summary workflow.</summary>
public static class AdaptiveBalanceSummaryWorkflow
{
    public static AdaptiveBalanceSummaryService NewAdaptiveBalanceSummaryService(IAdaptiveBalanceSummaryStore store, IClock clock)
    {
        return new AdaptiveBalanceSummaryService(
            store,
            AdaptiveBalanceSummaryRules.DefaultAdaptiveBalanceSummaryValidator(),
            clock,
            new List<string>());
    }
}
