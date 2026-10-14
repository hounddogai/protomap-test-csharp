namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive bonus pool checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveBonusPoolCheckpointService(IAdaptiveBonusPoolCheckpointStore Store, AdaptiveBonusPoolCheckpointValidator Validator, IClock Clock, List<string> Journal, AdaptiveBonusPoolCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive bonus pool checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveBonusPoolCheckpointStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive bonus pool checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveBonusPoolCheckpointStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive bonus pool checkpoint {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of adaptive bonus pool checkpoint workflow.</summary>
public static class AdaptiveBonusPoolCheckpointWorkflow
{
    public static AdaptiveBonusPoolCheckpointService NewAdaptiveBonusPoolCheckpointService(IAdaptiveBonusPoolCheckpointStore store, IClock clock)
    {
        return new AdaptiveBonusPoolCheckpointService(
            store,
            AdaptiveBonusPoolCheckpointRules.DefaultAdaptiveBonusPoolCheckpointValidator(),
            clock,
            new List<string>(),
            AdaptiveBonusPoolCheckpointEngine.DefaultAdaptiveBonusPoolCheckpointEngine());
    }
}
