namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal license seat adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalLicenseSeatAdjustmentService(ISeasonalLicenseSeatAdjustmentStore Store, SeasonalLicenseSeatAdjustmentValidator Validator, IClock Clock, List<string> Journal, SeasonalLicenseSeatAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal license seat adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalLicenseSeatAdjustmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal license seat adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalLicenseSeatAdjustmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal license seat adjustment {code} not found");
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

/// <summary>Functions of seasonal license seat adjustment workflow.</summary>
public static class SeasonalLicenseSeatAdjustmentWorkflow
{
    public static SeasonalLicenseSeatAdjustmentService NewSeasonalLicenseSeatAdjustmentService(ISeasonalLicenseSeatAdjustmentStore store, IClock clock, Registry flags)
    {
        return new SeasonalLicenseSeatAdjustmentService(
            store,
            SeasonalLicenseSeatAdjustmentRules.DefaultSeasonalLicenseSeatAdjustmentValidator(),
            clock,
            new List<string>(),
            SeasonalLicenseSeatAdjustmentEngine.DefaultSeasonalLicenseSeatAdjustmentEngine(flags));
    }
}
