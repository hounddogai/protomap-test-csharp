namespace Acme.Workforce.Compliance.TrainingDue.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted training due assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedTrainingDueAssignmentWorkflow(IWeightedTrainingDueAssignmentStore Store, WeightedTrainingDueAssignmentValidator Validator, IClock Clock, List<string> Journal, WeightedTrainingDueAssignmentEngine Engine)
{
    public static WeightedTrainingDueAssignmentWorkflow NewWeightedTrainingDueAssignmentWorkflow(IWeightedTrainingDueAssignmentStore store, IClock clock)
    {
        return new WeightedTrainingDueAssignmentWorkflow(
            store,
            WeightedTrainingDueAssignmentRules.DefaultWeightedTrainingDueAssignmentValidator(),
            clock,
            new List<string>(),
            WeightedTrainingDueAssignmentEngine.DefaultWeightedTrainingDueAssignmentEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted training due assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedTrainingDueAssignmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted training due assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedTrainingDueAssignmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted training due assignment {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.StartsAt > cutoff)
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
