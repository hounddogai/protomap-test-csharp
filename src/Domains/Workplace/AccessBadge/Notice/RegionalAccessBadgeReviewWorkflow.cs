namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional access badge reviews through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalAccessBadgeReviewWorkflow(IRegionalAccessBadgeReviewStore Store, RegionalAccessBadgeReviewValidator Validator, IClock Clock, List<string> Journal)
{
    public static RegionalAccessBadgeReviewWorkflow NewRegionalAccessBadgeReviewWorkflow(IRegionalAccessBadgeReviewStore store, IClock clock)
    {
        return new RegionalAccessBadgeReviewWorkflow(
            store,
            RegionalAccessBadgeReviewRules.DefaultRegionalAccessBadgeReviewValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional access badge review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalAccessBadgeReviewStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional access badge review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalAccessBadgeReviewStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
