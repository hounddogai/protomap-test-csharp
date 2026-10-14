namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled training due variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledTrainingDueVarianceManager(IScheduledTrainingDueVarianceStore Store, ScheduledTrainingDueVarianceValidator Validator, IClock Clock, List<string> Journal, ScheduledTrainingDueVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled training due variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledTrainingDueVarianceStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled training due variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledTrainingDueVarianceStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled training due variance {code} not found");
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

/// <summary>Functions of scheduled training due variance workflow.</summary>
public static class ScheduledTrainingDueVarianceWorkflow
{
    public static ScheduledTrainingDueVarianceManager NewScheduledTrainingDueVarianceManager(IScheduledTrainingDueVarianceStore store, IClock clock, Registry flags)
    {
        return new ScheduledTrainingDueVarianceManager(
            store,
            ScheduledTrainingDueVarianceRules.DefaultScheduledTrainingDueVarianceValidator(),
            clock,
            new List<string>(),
            ScheduledTrainingDueVarianceEngine.DefaultScheduledTrainingDueVarianceEngine(flags));
    }
}
