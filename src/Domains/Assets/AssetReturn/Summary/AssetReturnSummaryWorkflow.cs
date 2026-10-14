namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves asset return summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnSummaryDesk(IAssetReturnSummaryStore Store, AssetReturnSummaryValidator Validator, IClock Clock, List<string> Journal, AssetReturnSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnSummaryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnSummaryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return summary {code} not found");
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

/// <summary>Functions of asset return summary workflow.</summary>
public static class AssetReturnSummaryWorkflow
{
    public static AssetReturnSummaryDesk NewAssetReturnSummaryDesk(IAssetReturnSummaryStore store, IClock clock)
    {
        return new AssetReturnSummaryDesk(
            store,
            AssetReturnSummaryRules.DefaultAssetReturnSummaryValidator(),
            clock,
            new List<string>(),
            AssetReturnSummaryEngine.DefaultAssetReturnSummaryEngine());
    }
}
