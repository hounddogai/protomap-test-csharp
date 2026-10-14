namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched offboarding allocations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedOffboardingAllocationDesk(IBatchedOffboardingAllocationStore Store, BatchedOffboardingAllocationValidator Validator, IClock Clock, List<string> Journal, BatchedOffboardingAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched offboarding allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedOffboardingAllocationStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched offboarding allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedOffboardingAllocationStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched offboarding allocation {code} not found");
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

/// <summary>Functions of batched offboarding allocation workflow.</summary>
public static class BatchedOffboardingAllocationWorkflow
{
    public static BatchedOffboardingAllocationDesk NewBatchedOffboardingAllocationDesk(IBatchedOffboardingAllocationStore store, IClock clock, Registry flags)
    {
        return new BatchedOffboardingAllocationDesk(
            store,
            BatchedOffboardingAllocationRules.DefaultBatchedOffboardingAllocationValidator(),
            clock,
            new List<string>(),
            BatchedOffboardingAllocationEngine.DefaultBatchedOffboardingAllocationEngine(flags));
    }
}
