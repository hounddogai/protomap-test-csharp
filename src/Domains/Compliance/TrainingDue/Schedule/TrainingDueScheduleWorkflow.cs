namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves training due schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDueScheduleCoordinator(ITrainingDueScheduleStore Store, TrainingDueScheduleValidator Validator, IClock Clock, List<string> Journal, TrainingDueScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDueScheduleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDueScheduleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due schedule {code} not found");
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

/// <summary>Functions of training due schedule workflow.</summary>
public static class TrainingDueScheduleWorkflow
{
    public static TrainingDueScheduleCoordinator NewTrainingDueScheduleCoordinator(ITrainingDueScheduleStore store, IClock clock, Registry flags)
    {
        return new TrainingDueScheduleCoordinator(
            store,
            TrainingDueScheduleRules.DefaultTrainingDueScheduleValidator(),
            clock,
            new List<string>(),
            TrainingDueScheduleEngine.DefaultTrainingDueScheduleEngine(flags));
    }
}
