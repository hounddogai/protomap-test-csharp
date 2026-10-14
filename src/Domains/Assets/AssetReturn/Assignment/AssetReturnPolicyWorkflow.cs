namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves asset return policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnPolicyWorkflow(IAssetReturnPolicyStore Store, AssetReturnPolicyValidator Validator, IClock Clock, List<string> Journal, AssetReturnPolicyEngine Engine)
{
    public static AssetReturnPolicyWorkflow NewAssetReturnPolicyWorkflow(IAssetReturnPolicyStore store, IClock clock, Registry flags)
    {
        return new AssetReturnPolicyWorkflow(
            store,
            AssetReturnPolicyRules.DefaultAssetReturnPolicyValidator(),
            clock,
            new List<string>(),
            AssetReturnPolicyEngine.DefaultAssetReturnPolicyEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnPolicyStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnPolicyStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return policy {code} not found");
        }
        var result = Engine.Compute(item);
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
