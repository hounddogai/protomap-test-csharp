namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves training due scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDueScoreWorkflow(ITrainingDueScoreStore Store, TrainingDueScoreValidator Validator, IClock Clock, List<string> Journal)
{
    public static TrainingDueScoreWorkflow NewTrainingDueScoreWorkflow(ITrainingDueScoreStore store, IClock clock)
    {
        return new TrainingDueScoreWorkflow(
            store,
            TrainingDueScoreRules.DefaultTrainingDueScoreValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDueScoreStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDueScoreStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
