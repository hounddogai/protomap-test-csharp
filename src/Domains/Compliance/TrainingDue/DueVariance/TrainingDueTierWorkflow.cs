namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves training due tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDueTierManager(ITrainingDueTierStore Store, TrainingDueTierValidator Validator, IClock Clock, List<string> Journal, TrainingDueTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDueTierStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDueTierStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due tier {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of training due tier workflow.</summary>
public static class TrainingDueTierWorkflow
{
    public static TrainingDueTierManager NewTrainingDueTierManager(ITrainingDueTierStore store, IClock clock)
    {
        return new TrainingDueTierManager(
            store,
            TrainingDueTierRules.DefaultTrainingDueTierValidator(),
            clock,
            new List<string>(),
            TrainingDueTierEngine.DefaultTrainingDueTierEngine());
    }
}
