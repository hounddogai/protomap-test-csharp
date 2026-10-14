namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global feedback variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalFeedbackVarianceCoordinator(IGlobalFeedbackVarianceStore Store, GlobalFeedbackVarianceValidator Validator, IClock Clock, List<string> Journal, GlobalFeedbackVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global feedback variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalFeedbackVarianceStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global feedback variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalFeedbackVarianceStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global feedback variance {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of global feedback variance workflow.</summary>
public static class GlobalFeedbackVarianceWorkflow
{
    public static GlobalFeedbackVarianceCoordinator NewGlobalFeedbackVarianceCoordinator(IGlobalFeedbackVarianceStore store, IClock clock)
    {
        return new GlobalFeedbackVarianceCoordinator(
            store,
            GlobalFeedbackVarianceRules.DefaultGlobalFeedbackVarianceValidator(),
            clock,
            new List<string>(),
            GlobalFeedbackVarianceEngine.DefaultGlobalFeedbackVarianceEngine());
    }
}
