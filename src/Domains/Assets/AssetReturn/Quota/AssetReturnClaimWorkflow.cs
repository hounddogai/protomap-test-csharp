namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves asset return claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnClaimWorkflow(IAssetReturnClaimStore Store, AssetReturnClaimValidator Validator, IClock Clock, List<string> Journal, AssetReturnClaimEngine Engine)
{
    public static AssetReturnClaimWorkflow NewAssetReturnClaimWorkflow(IAssetReturnClaimStore store, IClock clock, Registry flags)
    {
        return new AssetReturnClaimWorkflow(
            store,
            AssetReturnClaimRules.DefaultAssetReturnClaimValidator(),
            clock,
            new List<string>(),
            AssetReturnClaimEngine.DefaultAssetReturnClaimEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnClaimStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnClaimStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return claim {code} not found");
        }
        var result = Engine.Price(item);
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
