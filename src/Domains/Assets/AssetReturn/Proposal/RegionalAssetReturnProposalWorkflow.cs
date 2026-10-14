namespace Acme.Workforce.Assets.AssetReturn.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional asset return proposals through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalAssetReturnProposalCoordinator(IRegionalAssetReturnProposalStore Store, RegionalAssetReturnProposalValidator Validator, IClock Clock, List<string> Journal, RegionalAssetReturnProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional asset return proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalAssetReturnProposalStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional asset return proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalAssetReturnProposalStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional asset return proposal {code} not found");
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

/// <summary>Functions of regional asset return proposal workflow.</summary>
public static class RegionalAssetReturnProposalWorkflow
{
    public static RegionalAssetReturnProposalCoordinator NewRegionalAssetReturnProposalCoordinator(IRegionalAssetReturnProposalStore store, IClock clock, Registry flags)
    {
        return new RegionalAssetReturnProposalCoordinator(
            store,
            RegionalAssetReturnProposalRules.DefaultRegionalAssetReturnProposalValidator(),
            clock,
            new List<string>(),
            RegionalAssetReturnProposalEngine.DefaultRegionalAssetReturnProposalEngine(flags));
    }
}
