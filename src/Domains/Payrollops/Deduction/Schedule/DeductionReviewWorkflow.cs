namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves deduction reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionReviewManager(IDeductionReviewStore Store, DeductionReviewValidator Validator, IClock Clock, List<string> Journal, DeductionReviewEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionReviewStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionReviewStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction review {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of deduction review workflow.</summary>
public static class DeductionReviewWorkflow
{
    public static DeductionReviewManager NewDeductionReviewManager(IDeductionReviewStore store, IClock clock, Registry flags)
    {
        return new DeductionReviewManager(
            store,
            DeductionReviewRules.DefaultDeductionReviewValidator(),
            clock,
            new List<string>(),
            DeductionReviewEngine.DefaultDeductionReviewEngine(flags));
    }
}
