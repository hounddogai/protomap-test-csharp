namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional asset return reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalAssetReturnReservationCoordinator(IRegionalAssetReturnReservationStore Store, RegionalAssetReturnReservationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional asset return reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalAssetReturnReservationStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional asset return reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalAssetReturnReservationStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of regional asset return reservation workflow.</summary>
public static class RegionalAssetReturnReservationWorkflow
{
    public static RegionalAssetReturnReservationCoordinator NewRegionalAssetReturnReservationCoordinator(IRegionalAssetReturnReservationStore store, IClock clock)
    {
        return new RegionalAssetReturnReservationCoordinator(
            store,
            RegionalAssetReturnReservationRules.DefaultRegionalAssetReturnReservationValidator(),
            clock,
            new List<string>());
    }
}
