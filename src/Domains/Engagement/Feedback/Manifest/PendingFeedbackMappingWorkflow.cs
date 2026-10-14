namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending feedback mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingFeedbackMappingService(IPendingFeedbackMappingStore Store, PendingFeedbackMappingValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending feedback mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingFeedbackMappingStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending feedback mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingFeedbackMappingStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of pending feedback mapping workflow.</summary>
public static class PendingFeedbackMappingWorkflow
{
    public static PendingFeedbackMappingService NewPendingFeedbackMappingService(IPendingFeedbackMappingStore store, IClock clock)
    {
        return new PendingFeedbackMappingService(
            store,
            PendingFeedbackMappingRules.DefaultPendingFeedbackMappingValidator(),
            clock,
            new List<string>());
    }
}
