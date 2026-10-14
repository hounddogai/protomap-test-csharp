namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched recognition drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedRecognitionDraftManager(IBatchedRecognitionDraftStore Store, BatchedRecognitionDraftValidator Validator, IClock Clock, List<string> Journal, BatchedRecognitionDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched recognition draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedRecognitionDraftStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched recognition draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedRecognitionDraftStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched recognition draft {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of batched recognition draft workflow.</summary>
public static class BatchedRecognitionDraftWorkflow
{
    public static BatchedRecognitionDraftManager NewBatchedRecognitionDraftManager(IBatchedRecognitionDraftStore store, IClock clock, Registry flags)
    {
        return new BatchedRecognitionDraftManager(
            store,
            BatchedRecognitionDraftRules.DefaultBatchedRecognitionDraftValidator(),
            clock,
            new List<string>(),
            BatchedRecognitionDraftEngine.DefaultBatchedRecognitionDraftEngine(flags));
    }
}
