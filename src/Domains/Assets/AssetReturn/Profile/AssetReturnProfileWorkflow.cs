namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves asset return profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnProfileDesk(IAssetReturnProfileStore Store, AssetReturnProfileValidator Validator, IClock Clock, List<string> Journal, AssetReturnProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnProfileStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnProfileStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return profile {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of asset return profile workflow.</summary>
public static class AssetReturnProfileWorkflow
{
    public static AssetReturnProfileDesk NewAssetReturnProfileDesk(IAssetReturnProfileStore store, IClock clock, Registry flags)
    {
        return new AssetReturnProfileDesk(
            store,
            AssetReturnProfileRules.DefaultAssetReturnProfileValidator(),
            clock,
            new List<string>(),
            AssetReturnProfileEngine.DefaultAssetReturnProfileEngine(flags));
    }
}
