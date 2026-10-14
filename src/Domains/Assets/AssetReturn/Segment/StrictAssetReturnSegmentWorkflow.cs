namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict asset return segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictAssetReturnSegmentDesk(IStrictAssetReturnSegmentStore Store, StrictAssetReturnSegmentValidator Validator, IClock Clock, List<string> Journal, StrictAssetReturnSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict asset return segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictAssetReturnSegmentStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict asset return segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictAssetReturnSegmentStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict asset return segment {code} not found");
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

/// <summary>Functions of strict asset return segment workflow.</summary>
public static class StrictAssetReturnSegmentWorkflow
{
    public static StrictAssetReturnSegmentDesk NewStrictAssetReturnSegmentDesk(IStrictAssetReturnSegmentStore store, IClock clock)
    {
        return new StrictAssetReturnSegmentDesk(
            store,
            StrictAssetReturnSegmentRules.DefaultStrictAssetReturnSegmentValidator(),
            clock,
            new List<string>(),
            StrictAssetReturnSegmentEngine.DefaultStrictAssetReturnSegmentEngine());
    }
}
