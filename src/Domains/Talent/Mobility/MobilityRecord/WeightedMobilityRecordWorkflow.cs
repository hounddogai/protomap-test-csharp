namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted mobility records through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedMobilityRecordService(IWeightedMobilityRecordStore Store, WeightedMobilityRecordValidator Validator, IClock Clock, List<string> Journal, WeightedMobilityRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted mobility record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedMobilityRecordStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted mobility record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedMobilityRecordStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted mobility record {code} not found");
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

/// <summary>Functions of weighted mobility record workflow.</summary>
public static class WeightedMobilityRecordWorkflow
{
    public static WeightedMobilityRecordService NewWeightedMobilityRecordService(IWeightedMobilityRecordStore store, IClock clock, Registry flags)
    {
        return new WeightedMobilityRecordService(
            store,
            WeightedMobilityRecordRules.DefaultWeightedMobilityRecordValidator(),
            clock,
            new List<string>(),
            WeightedMobilityRecordEngine.DefaultWeightedMobilityRecordEngine(flags));
    }
}
