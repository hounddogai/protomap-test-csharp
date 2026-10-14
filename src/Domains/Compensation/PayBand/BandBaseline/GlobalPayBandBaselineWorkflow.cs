namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global pay band baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalPayBandBaselineManager(IGlobalPayBandBaselineStore Store, GlobalPayBandBaselineValidator Validator, IClock Clock, List<string> Journal, GlobalPayBandBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pay band baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalPayBandBaselineStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pay band baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalPayBandBaselineStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pay band baseline {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of global pay band baseline workflow.</summary>
public static class GlobalPayBandBaselineWorkflow
{
    public static GlobalPayBandBaselineManager NewGlobalPayBandBaselineManager(IGlobalPayBandBaselineStore store, IClock clock, Registry flags)
    {
        return new GlobalPayBandBaselineManager(
            store,
            GlobalPayBandBaselineRules.DefaultGlobalPayBandBaselineValidator(),
            clock,
            new List<string>(),
            GlobalPayBandBaselineEngine.DefaultGlobalPayBandBaselineEngine(flags));
    }
}
