namespace Acme.Workforce.Absence.Accrual.AccrualManifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves accrual reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualReviewService(IAccrualReviewStore Store, AccrualReviewValidator Validator, IClock Clock, List<string> Journal, AccrualReviewEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualReviewStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualReviewStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual review {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of accrual review workflow.</summary>
public static class AccrualReviewWorkflow
{
    public static AccrualReviewService NewAccrualReviewService(IAccrualReviewStore store, IClock clock)
    {
        return new AccrualReviewService(
            store,
            AccrualReviewRules.DefaultAccrualReviewValidator(),
            clock,
            new List<string>(),
            AccrualReviewEngine.DefaultAccrualReviewEngine());
    }
}
