namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global pay band allocations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalPayBandAllocationWorkflow(IGlobalPayBandAllocationStore Store, GlobalPayBandAllocationValidator Validator, IClock Clock, List<string> Journal, GlobalPayBandAllocationEngine Engine)
{
    public static GlobalPayBandAllocationWorkflow NewGlobalPayBandAllocationWorkflow(IGlobalPayBandAllocationStore store, IClock clock, Registry flags)
    {
        return new GlobalPayBandAllocationWorkflow(
            store,
            GlobalPayBandAllocationRules.DefaultGlobalPayBandAllocationValidator(),
            clock,
            new List<string>(),
            GlobalPayBandAllocationEngine.DefaultGlobalPayBandAllocationEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pay band allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalPayBandAllocationStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pay band allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalPayBandAllocationStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pay band allocation {code} not found");
        }
        var result = Engine.Compute(item);
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
