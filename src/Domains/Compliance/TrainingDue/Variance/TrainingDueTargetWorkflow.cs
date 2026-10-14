namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves training due targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDueTargetWorkflow(ITrainingDueTargetStore Store, TrainingDueTargetValidator Validator, IClock Clock, List<string> Journal)
{
    public static TrainingDueTargetWorkflow NewTrainingDueTargetWorkflow(ITrainingDueTargetStore store, IClock clock)
    {
        return new TrainingDueTargetWorkflow(
            store,
            TrainingDueTargetRules.DefaultTrainingDueTargetValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDueTargetStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDueTargetStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
