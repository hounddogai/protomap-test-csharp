namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive cost model reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveCostModelReservationManager(IAdaptiveCostModelReservationStore Store, AdaptiveCostModelReservationValidator Validator, IClock Clock, List<string> Journal, AdaptiveCostModelReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive cost model reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveCostModelReservationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive cost model reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveCostModelReservationStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive cost model reservation {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of adaptive cost model reservation workflow.</summary>
public static class AdaptiveCostModelReservationWorkflow
{
    public static AdaptiveCostModelReservationManager NewAdaptiveCostModelReservationManager(IAdaptiveCostModelReservationStore store, IClock clock)
    {
        return new AdaptiveCostModelReservationManager(
            store,
            AdaptiveCostModelReservationRules.DefaultAdaptiveCostModelReservationValidator(),
            clock,
            new List<string>(),
            AdaptiveCostModelReservationEngine.DefaultAdaptiveCostModelReservationEngine());
    }
}
