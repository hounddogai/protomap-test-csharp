namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay band slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandSlotService(IPayBandSlotStore Store, PayBandSlotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandSlotStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandSlotStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of pay band slot workflow.</summary>
public static class PayBandSlotWorkflow
{
    public static PayBandSlotService NewPayBandSlotService(IPayBandSlotStore store, IClock clock)
    {
        return new PayBandSlotService(
            store,
            PayBandSlotRules.DefaultPayBandSlotValidator(),
            clock,
            new List<string>());
    }
}
