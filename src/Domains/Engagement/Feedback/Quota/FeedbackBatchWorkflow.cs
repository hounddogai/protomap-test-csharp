namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves feedback batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record FeedbackBatchService(IFeedbackBatchStore Store, FeedbackBatchValidator Validator, IClock Clock, List<string> Journal, FeedbackBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FeedbackBatchStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FeedbackBatchStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback batch {code} not found");
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

/// <summary>Functions of feedback batch workflow.</summary>
public static class FeedbackBatchWorkflow
{
    public static FeedbackBatchService NewFeedbackBatchService(IFeedbackBatchStore store, IClock clock)
    {
        return new FeedbackBatchService(
            store,
            FeedbackBatchRules.DefaultFeedbackBatchValidator(),
            clock,
            new List<string>(),
            FeedbackBatchEngine.DefaultFeedbackBatchEngine());
    }
}
