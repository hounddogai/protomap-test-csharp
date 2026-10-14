namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves asset return reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnReservationManager(IAssetReturnReservationStore Store, AssetReturnReservationValidator Validator, IClock Clock, List<string> Journal, AssetReturnReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnReservationStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnReservationStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return reservation {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of asset return reservation workflow.</summary>
public static class AssetReturnReservationWorkflow
{
    public static AssetReturnReservationManager NewAssetReturnReservationManager(IAssetReturnReservationStore store, IClock clock)
    {
        return new AssetReturnReservationManager(
            store,
            AssetReturnReservationRules.DefaultAssetReturnReservationValidator(),
            clock,
            new List<string>(),
            AssetReturnReservationEngine.DefaultAssetReturnReservationEngine());
    }
}
