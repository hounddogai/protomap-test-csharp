namespace Acme.Workforce.Talent.Mobility.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy mobility segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyMobilitySegmentCoordinator(ILegacyMobilitySegmentStore Store, LegacyMobilitySegmentValidator Validator, IClock Clock, List<string> Journal, LegacyMobilitySegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy mobility segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyMobilitySegmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy mobility segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyMobilitySegmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy mobility segment {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of legacy mobility segment workflow.</summary>
public static class LegacyMobilitySegmentWorkflow
{
    public static LegacyMobilitySegmentCoordinator NewLegacyMobilitySegmentCoordinator(ILegacyMobilitySegmentStore store, IClock clock, Registry flags)
    {
        return new LegacyMobilitySegmentCoordinator(
            store,
            LegacyMobilitySegmentRules.DefaultLegacyMobilitySegmentValidator(),
            clock,
            new List<string>(),
            LegacyMobilitySegmentEngine.DefaultLegacyMobilitySegmentEngine(flags));
    }
}
