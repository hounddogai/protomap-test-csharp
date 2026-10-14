namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling training due quotas through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingTrainingDueQuotaCoordinator(IRollingTrainingDueQuotaStore Store, RollingTrainingDueQuotaValidator Validator, IClock Clock, List<string> Journal, RollingTrainingDueQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling training due quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingTrainingDueQuotaStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling training due quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingTrainingDueQuotaStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling training due quota {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of rolling training due quota workflow.</summary>
public static class RollingTrainingDueQuotaWorkflow
{
    public static RollingTrainingDueQuotaCoordinator NewRollingTrainingDueQuotaCoordinator(IRollingTrainingDueQuotaStore store, IClock clock, Registry flags)
    {
        return new RollingTrainingDueQuotaCoordinator(
            store,
            RollingTrainingDueQuotaRules.DefaultRollingTrainingDueQuotaValidator(),
            clock,
            new List<string>(),
            RollingTrainingDueQuotaEngine.DefaultRollingTrainingDueQuotaEngine(flags));
    }
}
