namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched training due tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedTrainingDueTierWorkflow(IBatchedTrainingDueTierStore Store, BatchedTrainingDueTierValidator Validator, IClock Clock, List<string> Journal, BatchedTrainingDueTierEngine Engine)
{
    public static BatchedTrainingDueTierWorkflow NewBatchedTrainingDueTierWorkflow(IBatchedTrainingDueTierStore store, IClock clock, Registry flags)
    {
        return new BatchedTrainingDueTierWorkflow(
            store,
            BatchedTrainingDueTierRules.DefaultBatchedTrainingDueTierValidator(),
            clock,
            new List<string>(),
            BatchedTrainingDueTierEngine.DefaultBatchedTrainingDueTierEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched training due tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedTrainingDueTierStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched training due tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedTrainingDueTierStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched training due tier {code} not found");
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
            if (item.EndsAt > cutoff)
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
