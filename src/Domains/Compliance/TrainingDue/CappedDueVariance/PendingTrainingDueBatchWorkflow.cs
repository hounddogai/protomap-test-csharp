namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending training due batches through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingTrainingDueBatchManager(IPendingTrainingDueBatchStore Store, PendingTrainingDueBatchValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending training due batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingTrainingDueBatchStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending training due batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingTrainingDueBatchStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of pending training due batch workflow.</summary>
public static class PendingTrainingDueBatchWorkflow
{
    public static PendingTrainingDueBatchManager NewPendingTrainingDueBatchManager(IPendingTrainingDueBatchStore store, IClock clock)
    {
        return new PendingTrainingDueBatchManager(
            store,
            PendingTrainingDueBatchRules.DefaultPendingTrainingDueBatchValidator(),
            clock,
            new List<string>());
    }
}
