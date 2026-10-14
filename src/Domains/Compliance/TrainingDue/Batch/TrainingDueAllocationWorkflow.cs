namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves training due allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDueAllocationWorkflow(ITrainingDueAllocationStore Store, TrainingDueAllocationValidator Validator, IClock Clock, List<string> Journal, TrainingDueAllocationEngine Engine)
{
    public static TrainingDueAllocationWorkflow NewTrainingDueAllocationWorkflow(ITrainingDueAllocationStore store, IClock clock, Registry flags)
    {
        return new TrainingDueAllocationWorkflow(
            store,
            TrainingDueAllocationRules.DefaultTrainingDueAllocationValidator(),
            clock,
            new List<string>(),
            TrainingDueAllocationEngine.DefaultTrainingDueAllocationEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDueAllocationStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDueAllocationStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due allocation {code} not found");
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
