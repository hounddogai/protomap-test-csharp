namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive headcount records through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveHeadcountRecordManager(IAdaptiveHeadcountRecordStore Store, AdaptiveHeadcountRecordValidator Validator, IClock Clock, List<string> Journal, AdaptiveHeadcountRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive headcount record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveHeadcountRecordStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive headcount record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveHeadcountRecordStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive headcount record {code} not found");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of adaptive headcount record workflow.</summary>
public static class AdaptiveHeadcountRecordWorkflow
{
    public static AdaptiveHeadcountRecordManager NewAdaptiveHeadcountRecordManager(IAdaptiveHeadcountRecordStore store, IClock clock, Registry flags)
    {
        return new AdaptiveHeadcountRecordManager(
            store,
            AdaptiveHeadcountRecordRules.DefaultAdaptiveHeadcountRecordValidator(),
            clock,
            new List<string>(),
            AdaptiveHeadcountRecordEngine.DefaultAdaptiveHeadcountRecordEngine(flags));
    }
}
