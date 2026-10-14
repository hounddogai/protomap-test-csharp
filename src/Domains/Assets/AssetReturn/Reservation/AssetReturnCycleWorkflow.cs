namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves asset return cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnCycleWorkflow(IAssetReturnCycleStore Store, AssetReturnCycleValidator Validator, IClock Clock, List<string> Journal, AssetReturnCycleEngine Engine)
{
    public static AssetReturnCycleWorkflow NewAssetReturnCycleWorkflow(IAssetReturnCycleStore store, IClock clock)
    {
        return new AssetReturnCycleWorkflow(
            store,
            AssetReturnCycleRules.DefaultAssetReturnCycleValidator(),
            clock,
            new List<string>(),
            AssetReturnCycleEngine.DefaultAssetReturnCycleEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnCycleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnCycleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return cycle {code} not found");
        }
        var result = Engine.Compute(item);
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
