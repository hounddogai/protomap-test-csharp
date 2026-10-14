namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched feedback scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedFeedbackScoreWorkflow(IBatchedFeedbackScoreStore Store, BatchedFeedbackScoreValidator Validator, IClock Clock, List<string> Journal, BatchedFeedbackScoreEngine Engine)
{
    public static BatchedFeedbackScoreWorkflow NewBatchedFeedbackScoreWorkflow(IBatchedFeedbackScoreStore store, IClock clock)
    {
        return new BatchedFeedbackScoreWorkflow(
            store,
            BatchedFeedbackScoreRules.DefaultBatchedFeedbackScoreValidator(),
            clock,
            new List<string>(),
            BatchedFeedbackScoreEngine.DefaultBatchedFeedbackScoreEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched feedback score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedFeedbackScoreStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched feedback score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedFeedbackScoreStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched feedback score {code} not found");
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
