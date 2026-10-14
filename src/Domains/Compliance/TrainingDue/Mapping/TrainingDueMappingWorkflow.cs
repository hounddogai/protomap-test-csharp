namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves training due mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDueMappingDesk(ITrainingDueMappingStore Store, TrainingDueMappingValidator Validator, IClock Clock, List<string> Journal, TrainingDueMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDueMappingStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDueMappingStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due mapping {code} not found");
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

/// <summary>Functions of training due mapping workflow.</summary>
public static class TrainingDueMappingWorkflow
{
    public static TrainingDueMappingDesk NewTrainingDueMappingDesk(ITrainingDueMappingStore store, IClock clock, Registry flags)
    {
        return new TrainingDueMappingDesk(
            store,
            TrainingDueMappingRules.DefaultTrainingDueMappingValidator(),
            clock,
            new List<string>(),
            TrainingDueMappingEngine.DefaultTrainingDueMappingEngine(flags));
    }
}
