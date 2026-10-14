namespace Acme.Workforce.Compliance.TrainingDue.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional training due reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalTrainingDueReservationCoordinator(IRegionalTrainingDueReservationStore Store, RegionalTrainingDueReservationValidator Validator, IClock Clock, List<string> Journal, RegionalTrainingDueReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional training due reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalTrainingDueReservationStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional training due reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalTrainingDueReservationStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional training due reservation {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of regional training due reservation workflow.</summary>
public static class RegionalTrainingDueReservationWorkflow
{
    public static RegionalTrainingDueReservationCoordinator NewRegionalTrainingDueReservationCoordinator(IRegionalTrainingDueReservationStore store, IClock clock)
    {
        return new RegionalTrainingDueReservationCoordinator(
            store,
            RegionalTrainingDueReservationRules.DefaultRegionalTrainingDueReservationValidator(),
            clock,
            new List<string>(),
            RegionalTrainingDueReservationEngine.DefaultRegionalTrainingDueReservationEngine());
    }
}
