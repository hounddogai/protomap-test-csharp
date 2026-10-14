namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves training due profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDueProfileCoordinator(ITrainingDueProfileStore Store, TrainingDueProfileValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDueProfileStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDueProfileStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of training due profile workflow.</summary>
public static class TrainingDueProfileWorkflow
{
    public static TrainingDueProfileCoordinator NewTrainingDueProfileCoordinator(ITrainingDueProfileStore store, IClock clock)
    {
        return new TrainingDueProfileCoordinator(
            store,
            TrainingDueProfileRules.DefaultTrainingDueProfileValidator(),
            clock,
            new List<string>());
    }
}
