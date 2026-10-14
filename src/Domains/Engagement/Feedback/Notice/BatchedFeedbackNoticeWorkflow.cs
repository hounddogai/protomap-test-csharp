namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched feedback notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedFeedbackNoticeService(IBatchedFeedbackNoticeStore Store, BatchedFeedbackNoticeValidator Validator, IClock Clock, List<string> Journal, BatchedFeedbackNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched feedback notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedFeedbackNoticeStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched feedback notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedFeedbackNoticeStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched feedback notice {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of batched feedback notice workflow.</summary>
public static class BatchedFeedbackNoticeWorkflow
{
    public static BatchedFeedbackNoticeService NewBatchedFeedbackNoticeService(IBatchedFeedbackNoticeStore store, IClock clock, Registry flags)
    {
        return new BatchedFeedbackNoticeService(
            store,
            BatchedFeedbackNoticeRules.DefaultBatchedFeedbackNoticeValidator(),
            clock,
            new List<string>(),
            BatchedFeedbackNoticeEngine.DefaultBatchedFeedbackNoticeEngine(flags));
    }
}
