namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy equity grant segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LegacyEquityGrantSegmentDesk(ILegacyEquityGrantSegmentStore Store, LegacyEquityGrantSegmentValidator Validator, IClock Clock, List<string> Journal, LegacyEquityGrantSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy equity grant segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyEquityGrantSegmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy equity grant segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyEquityGrantSegmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy equity grant segment {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of legacy equity grant segment workflow.</summary>
public static class LegacyEquityGrantSegmentWorkflow
{
    public static LegacyEquityGrantSegmentDesk NewLegacyEquityGrantSegmentDesk(ILegacyEquityGrantSegmentStore store, IClock clock)
    {
        return new LegacyEquityGrantSegmentDesk(
            store,
            LegacyEquityGrantSegmentRules.DefaultLegacyEquityGrantSegmentValidator(),
            clock,
            new List<string>(),
            LegacyEquityGrantSegmentEngine.DefaultLegacyEquityGrantSegmentEngine());
    }
}
