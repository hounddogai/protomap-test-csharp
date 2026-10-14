namespace Acme.Workforce.Assets.AssetReturn.ReturnReservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves asset return entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnEntryWorkflow(IAssetReturnEntryStore Store, AssetReturnEntryValidator Validator, IClock Clock, List<string> Journal, AssetReturnEntryEngine Engine)
{
    public static AssetReturnEntryWorkflow NewAssetReturnEntryWorkflow(IAssetReturnEntryStore store, IClock clock)
    {
        return new AssetReturnEntryWorkflow(
            store,
            AssetReturnEntryRules.DefaultAssetReturnEntryValidator(),
            clock,
            new List<string>(),
            AssetReturnEntryEngine.DefaultAssetReturnEntryEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnEntryStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnEntryStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return entry {code} not found");
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
