namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves training due plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDuePlanService(ITrainingDuePlanStore Store, TrainingDuePlanValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDuePlanStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDuePlanStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of training due plan workflow.</summary>
public static class TrainingDuePlanWorkflow
{
    public static TrainingDuePlanService NewTrainingDuePlanService(ITrainingDuePlanStore store, IClock clock)
    {
        return new TrainingDuePlanService(
            store,
            TrainingDuePlanRules.DefaultTrainingDuePlanValidator(),
            clock,
            new List<string>());
    }
}
