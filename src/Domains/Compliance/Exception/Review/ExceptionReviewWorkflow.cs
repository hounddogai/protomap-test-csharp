namespace Acme.Workforce.Compliance.Exception.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves exception reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionReviewWorkflow(IExceptionReviewStore Store, ExceptionReviewValidator Validator, IClock Clock, List<string> Journal, ExceptionReviewEngine Engine)
{
    public static ExceptionReviewWorkflow NewExceptionReviewWorkflow(IExceptionReviewStore store, IClock clock)
    {
        return new ExceptionReviewWorkflow(
            store,
            ExceptionReviewRules.DefaultExceptionReviewValidator(),
            clock,
            new List<string>(),
            ExceptionReviewEngine.DefaultExceptionReviewEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionReviewStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionReviewStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception review {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.StartsAt > cutoff)
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
