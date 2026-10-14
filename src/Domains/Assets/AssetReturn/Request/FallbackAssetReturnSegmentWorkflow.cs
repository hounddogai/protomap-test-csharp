namespace Acme.Workforce.Assets.AssetReturn.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback asset return segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackAssetReturnSegmentWorkflow(IFallbackAssetReturnSegmentStore Store, FallbackAssetReturnSegmentValidator Validator, IClock Clock, List<string> Journal, FallbackAssetReturnSegmentEngine Engine)
{
    public static FallbackAssetReturnSegmentWorkflow NewFallbackAssetReturnSegmentWorkflow(IFallbackAssetReturnSegmentStore store, IClock clock)
    {
        return new FallbackAssetReturnSegmentWorkflow(
            store,
            FallbackAssetReturnSegmentRules.DefaultFallbackAssetReturnSegmentValidator(),
            clock,
            new List<string>(),
            FallbackAssetReturnSegmentEngine.DefaultFallbackAssetReturnSegmentEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback asset return segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackAssetReturnSegmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback asset return segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackAssetReturnSegmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback asset return segment {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.EndsAt > cutoff)
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
