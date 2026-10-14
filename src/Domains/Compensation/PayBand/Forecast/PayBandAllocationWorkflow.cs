namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pay band allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandAllocationDesk(IPayBandAllocationStore Store, PayBandAllocationValidator Validator, IClock Clock, List<string> Journal, PayBandAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandAllocationStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandAllocationStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band allocation {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of pay band allocation workflow.</summary>
public static class PayBandAllocationWorkflow
{
    public static PayBandAllocationDesk NewPayBandAllocationDesk(IPayBandAllocationStore store, IClock clock, Registry flags)
    {
        return new PayBandAllocationDesk(
            store,
            PayBandAllocationRules.DefaultPayBandAllocationValidator(),
            clock,
            new List<string>(),
            PayBandAllocationEngine.DefaultPayBandAllocationEngine(flags));
    }
}
