namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback license seat records through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackLicenseSeatRecordService(IFallbackLicenseSeatRecordStore Store, FallbackLicenseSeatRecordValidator Validator, IClock Clock, List<string> Journal, FallbackLicenseSeatRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback license seat record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackLicenseSeatRecordStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback license seat record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackLicenseSeatRecordStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback license seat record {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of fallback license seat record workflow.</summary>
public static class FallbackLicenseSeatRecordWorkflow
{
    public static FallbackLicenseSeatRecordService NewFallbackLicenseSeatRecordService(IFallbackLicenseSeatRecordStore store, IClock clock, Registry flags)
    {
        return new FallbackLicenseSeatRecordService(
            store,
            FallbackLicenseSeatRecordRules.DefaultFallbackLicenseSeatRecordValidator(),
            clock,
            new List<string>(),
            FallbackLicenseSeatRecordEngine.DefaultFallbackLicenseSeatRecordEngine(flags));
    }
}
