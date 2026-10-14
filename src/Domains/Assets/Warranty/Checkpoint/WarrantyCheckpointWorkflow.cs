namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves warranty checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyCheckpointCoordinator(IWarrantyCheckpointStore Store, WarrantyCheckpointValidator Validator, IClock Clock, List<string> Journal, WarrantyCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyCheckpointStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyCheckpointStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty checkpoint {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of warranty checkpoint workflow.</summary>
public static class WarrantyCheckpointWorkflow
{
    public static WarrantyCheckpointCoordinator NewWarrantyCheckpointCoordinator(IWarrantyCheckpointStore store, IClock clock, Registry flags)
    {
        return new WarrantyCheckpointCoordinator(
            store,
            WarrantyCheckpointRules.DefaultWarrantyCheckpointValidator(),
            clock,
            new List<string>(),
            WarrantyCheckpointEngine.DefaultWarrantyCheckpointEngine(flags));
    }
}
