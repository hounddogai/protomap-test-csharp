namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched recognition slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedRecognitionSlotService(IBatchedRecognitionSlotStore Store, BatchedRecognitionSlotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched recognition slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedRecognitionSlotStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched recognition slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedRecognitionSlotStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of batched recognition slot workflow.</summary>
public static class BatchedRecognitionSlotWorkflow
{
    public static BatchedRecognitionSlotService NewBatchedRecognitionSlotService(IBatchedRecognitionSlotStore store, IClock clock)
    {
        return new BatchedRecognitionSlotService(
            store,
            BatchedRecognitionSlotRules.DefaultBatchedRecognitionSlotValidator(),
            clock,
            new List<string>());
    }
}
