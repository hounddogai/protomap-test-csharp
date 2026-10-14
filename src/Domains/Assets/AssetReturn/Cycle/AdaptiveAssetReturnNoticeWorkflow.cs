namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive asset return notices through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveAssetReturnNoticeWorkflow(IAdaptiveAssetReturnNoticeStore Store, AdaptiveAssetReturnNoticeValidator Validator, IClock Clock, List<string> Journal)
{
    public static AdaptiveAssetReturnNoticeWorkflow NewAdaptiveAssetReturnNoticeWorkflow(IAdaptiveAssetReturnNoticeStore store, IClock clock)
    {
        return new AdaptiveAssetReturnNoticeWorkflow(
            store,
            AdaptiveAssetReturnNoticeRules.DefaultAdaptiveAssetReturnNoticeValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive asset return notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveAssetReturnNoticeStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive asset return notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveAssetReturnNoticeStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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
