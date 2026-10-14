namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback training due batches through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackTrainingDueBatchService(IFallbackTrainingDueBatchStore Store, FallbackTrainingDueBatchValidator Validator, IClock Clock, List<string> Journal, FallbackTrainingDueBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback training due batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackTrainingDueBatchStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback training due batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackTrainingDueBatchStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback training due batch {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of fallback training due batch workflow.</summary>
public static class FallbackTrainingDueBatchWorkflow
{
    public static FallbackTrainingDueBatchService NewFallbackTrainingDueBatchService(IFallbackTrainingDueBatchStore store, IClock clock)
    {
        return new FallbackTrainingDueBatchService(
            store,
            FallbackTrainingDueBatchRules.DefaultFallbackTrainingDueBatchValidator(),
            clock,
            new List<string>(),
            FallbackTrainingDueBatchEngine.DefaultFallbackTrainingDueBatchEngine());
    }
}
