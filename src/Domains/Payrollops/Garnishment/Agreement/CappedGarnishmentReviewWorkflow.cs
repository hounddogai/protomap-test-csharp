namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped garnishment reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedGarnishmentReviewService(ICappedGarnishmentReviewStore Store, CappedGarnishmentReviewValidator Validator, IClock Clock, List<string> Journal, CappedGarnishmentReviewEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped garnishment review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedGarnishmentReviewStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped garnishment review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedGarnishmentReviewStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped garnishment review {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of capped garnishment review workflow.</summary>
public static class CappedGarnishmentReviewWorkflow
{
    public static CappedGarnishmentReviewService NewCappedGarnishmentReviewService(ICappedGarnishmentReviewStore store, IClock clock)
    {
        return new CappedGarnishmentReviewService(
            store,
            CappedGarnishmentReviewRules.DefaultCappedGarnishmentReviewValidator(),
            clock,
            new List<string>(),
            CappedGarnishmentReviewEngine.DefaultCappedGarnishmentReviewEngine());
    }
}
