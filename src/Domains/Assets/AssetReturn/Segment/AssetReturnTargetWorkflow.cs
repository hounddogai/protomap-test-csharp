namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves asset return targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnTargetDesk(IAssetReturnTargetStore Store, AssetReturnTargetValidator Validator, IClock Clock, List<string> Journal, AssetReturnTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnTargetStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnTargetStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return target {code} not found");
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

/// <summary>Functions of asset return target workflow.</summary>
public static class AssetReturnTargetWorkflow
{
    public static AssetReturnTargetDesk NewAssetReturnTargetDesk(IAssetReturnTargetStore store, IClock clock)
    {
        return new AssetReturnTargetDesk(
            store,
            AssetReturnTargetRules.DefaultAssetReturnTargetValidator(),
            clock,
            new List<string>(),
            AssetReturnTargetEngine.DefaultAssetReturnTargetEngine());
    }
}
