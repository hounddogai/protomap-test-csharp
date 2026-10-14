namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves holiday thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayThresholdManager(IHolidayThresholdStore Store, HolidayThresholdValidator Validator, IClock Clock, List<string> Journal, HolidayThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayThresholdStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayThresholdStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday threshold {code} not found");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of holiday threshold workflow.</summary>
public static class HolidayThresholdWorkflow
{
    public static HolidayThresholdManager NewHolidayThresholdManager(IHolidayThresholdStore store, IClock clock, Registry flags)
    {
        return new HolidayThresholdManager(
            store,
            HolidayThresholdRules.DefaultHolidayThresholdValidator(),
            clock,
            new List<string>(),
            HolidayThresholdEngine.DefaultHolidayThresholdEngine(flags));
    }
}
