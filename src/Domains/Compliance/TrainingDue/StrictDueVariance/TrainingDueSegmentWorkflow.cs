namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves training due segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDueSegmentCoordinator(ITrainingDueSegmentStore Store, TrainingDueSegmentValidator Validator, IClock Clock, List<string> Journal, TrainingDueSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDueSegmentStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDueSegmentStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due segment {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of training due segment workflow.</summary>
public static class TrainingDueSegmentWorkflow
{
    public static TrainingDueSegmentCoordinator NewTrainingDueSegmentCoordinator(ITrainingDueSegmentStore store, IClock clock)
    {
        return new TrainingDueSegmentCoordinator(
            store,
            TrainingDueSegmentRules.DefaultTrainingDueSegmentValidator(),
            clock,
            new List<string>(),
            TrainingDueSegmentEngine.DefaultTrainingDueSegmentEngine());
    }
}
