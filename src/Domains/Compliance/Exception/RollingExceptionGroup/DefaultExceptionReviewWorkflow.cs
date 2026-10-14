namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default exception reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultExceptionReviewCoordinator(IDefaultExceptionReviewStore Store, DefaultExceptionReviewValidator Validator, IClock Clock, List<string> Journal, DefaultExceptionReviewEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default exception review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultExceptionReviewStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default exception review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultExceptionReviewStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default exception review {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of default exception review workflow.</summary>
public static class DefaultExceptionReviewWorkflow
{
    public static DefaultExceptionReviewCoordinator NewDefaultExceptionReviewCoordinator(IDefaultExceptionReviewStore store, IClock clock)
    {
        return new DefaultExceptionReviewCoordinator(
            store,
            DefaultExceptionReviewRules.DefaultDefaultExceptionReviewValidator(),
            clock,
            new List<string>(),
            DefaultExceptionReviewEngine.DefaultDefaultExceptionReviewEngine());
    }
}
