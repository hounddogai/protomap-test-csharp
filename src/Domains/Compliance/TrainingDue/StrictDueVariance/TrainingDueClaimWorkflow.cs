namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves training due claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDueClaimDesk(ITrainingDueClaimStore Store, TrainingDueClaimValidator Validator, IClock Clock, List<string> Journal, TrainingDueClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDueClaimStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDueClaimStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due claim {code} not found");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of training due claim workflow.</summary>
public static class TrainingDueClaimWorkflow
{
    public static TrainingDueClaimDesk NewTrainingDueClaimDesk(ITrainingDueClaimStore store, IClock clock, Registry flags)
    {
        return new TrainingDueClaimDesk(
            store,
            TrainingDueClaimRules.DefaultTrainingDueClaimValidator(),
            clock,
            new List<string>(),
            TrainingDueClaimEngine.DefaultTrainingDueClaimEngine(flags));
    }
}
