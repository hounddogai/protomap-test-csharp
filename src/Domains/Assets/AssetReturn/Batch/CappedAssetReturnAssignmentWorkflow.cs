namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped asset return assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedAssetReturnAssignmentCoordinator(ICappedAssetReturnAssignmentStore Store, CappedAssetReturnAssignmentValidator Validator, IClock Clock, List<string> Journal, CappedAssetReturnAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped asset return assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedAssetReturnAssignmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped asset return assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedAssetReturnAssignmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped asset return assignment {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of capped asset return assignment workflow.</summary>
public static class CappedAssetReturnAssignmentWorkflow
{
    public static CappedAssetReturnAssignmentCoordinator NewCappedAssetReturnAssignmentCoordinator(ICappedAssetReturnAssignmentStore store, IClock clock, Registry flags)
    {
        return new CappedAssetReturnAssignmentCoordinator(
            store,
            CappedAssetReturnAssignmentRules.DefaultCappedAssetReturnAssignmentValidator(),
            clock,
            new List<string>(),
            CappedAssetReturnAssignmentEngine.DefaultCappedAssetReturnAssignmentEngine(flags));
    }
}
