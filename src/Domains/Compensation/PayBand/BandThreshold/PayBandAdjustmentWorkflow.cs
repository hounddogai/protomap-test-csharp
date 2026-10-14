namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay band adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandAdjustmentDesk(IPayBandAdjustmentStore Store, PayBandAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandAdjustmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandAdjustmentStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of pay band adjustment workflow.</summary>
public static class PayBandAdjustmentWorkflow
{
    public static PayBandAdjustmentDesk NewPayBandAdjustmentDesk(IPayBandAdjustmentStore store, IClock clock)
    {
        return new PayBandAdjustmentDesk(
            store,
            PayBandAdjustmentRules.DefaultPayBandAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
