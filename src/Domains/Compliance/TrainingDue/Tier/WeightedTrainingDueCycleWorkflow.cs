namespace Acme.Workforce.Compliance.TrainingDue.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted training due cycles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedTrainingDueCycleManager(IWeightedTrainingDueCycleStore Store, WeightedTrainingDueCycleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted training due cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedTrainingDueCycleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted training due cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedTrainingDueCycleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of weighted training due cycle workflow.</summary>
public static class WeightedTrainingDueCycleWorkflow
{
    public static WeightedTrainingDueCycleManager NewWeightedTrainingDueCycleManager(IWeightedTrainingDueCycleStore store, IClock clock)
    {
        return new WeightedTrainingDueCycleManager(
            store,
            WeightedTrainingDueCycleRules.DefaultWeightedTrainingDueCycleValidator(),
            clock,
            new List<string>());
    }
}
