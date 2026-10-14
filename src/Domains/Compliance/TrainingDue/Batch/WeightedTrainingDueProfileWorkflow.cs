namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted training due profiles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedTrainingDueProfileWorkflow(IWeightedTrainingDueProfileStore Store, WeightedTrainingDueProfileValidator Validator, IClock Clock, List<string> Journal, WeightedTrainingDueProfileEngine Engine)
{
    public static WeightedTrainingDueProfileWorkflow NewWeightedTrainingDueProfileWorkflow(IWeightedTrainingDueProfileStore store, IClock clock, Registry flags)
    {
        return new WeightedTrainingDueProfileWorkflow(
            store,
            WeightedTrainingDueProfileRules.DefaultWeightedTrainingDueProfileValidator(),
            clock,
            new List<string>(),
            WeightedTrainingDueProfileEngine.DefaultWeightedTrainingDueProfileEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted training due profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedTrainingDueProfileStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted training due profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedTrainingDueProfileStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted training due profile {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
