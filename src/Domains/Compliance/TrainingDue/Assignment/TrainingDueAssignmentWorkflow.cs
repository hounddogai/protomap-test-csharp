namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves training due assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDueAssignmentWorkflow(ITrainingDueAssignmentStore Store, TrainingDueAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    public static TrainingDueAssignmentWorkflow NewTrainingDueAssignmentWorkflow(ITrainingDueAssignmentStore store, IClock clock)
    {
        return new TrainingDueAssignmentWorkflow(
            store,
            TrainingDueAssignmentRules.DefaultTrainingDueAssignmentValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDueAssignmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDueAssignmentStatus.Closed);
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
