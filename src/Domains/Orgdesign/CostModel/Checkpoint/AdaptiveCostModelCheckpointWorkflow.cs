namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive cost model checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveCostModelCheckpointDesk(IAdaptiveCostModelCheckpointStore Store, AdaptiveCostModelCheckpointValidator Validator, IClock Clock, List<string> Journal, AdaptiveCostModelCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive cost model checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveCostModelCheckpointStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive cost model checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveCostModelCheckpointStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive cost model checkpoint {code} not found");
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

/// <summary>Functions of adaptive cost model checkpoint workflow.</summary>
public static class AdaptiveCostModelCheckpointWorkflow
{
    public static AdaptiveCostModelCheckpointDesk NewAdaptiveCostModelCheckpointDesk(IAdaptiveCostModelCheckpointStore store, IClock clock, Registry flags)
    {
        return new AdaptiveCostModelCheckpointDesk(
            store,
            AdaptiveCostModelCheckpointRules.DefaultAdaptiveCostModelCheckpointValidator(),
            clock,
            new List<string>(),
            AdaptiveCostModelCheckpointEngine.DefaultAdaptiveCostModelCheckpointEngine(flags));
    }
}
