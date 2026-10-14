namespace Acme.Workforce.Assets.AssetReturn.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves asset return proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnProposalDesk(IAssetReturnProposalStore Store, AssetReturnProposalValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnProposalStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnProposalStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of asset return proposal workflow.</summary>
public static class AssetReturnProposalWorkflow
{
    public static AssetReturnProposalDesk NewAssetReturnProposalDesk(IAssetReturnProposalStore store, IClock clock)
    {
        return new AssetReturnProposalDesk(
            store,
            AssetReturnProposalRules.DefaultAssetReturnProposalValidator(),
            clock,
            new List<string>());
    }
}
