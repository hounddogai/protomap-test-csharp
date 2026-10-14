namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay band records through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandRecordManager(IPayBandRecordStore Store, PayBandRecordValidator Validator, IClock Clock, List<string> Journal, PayBandRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandRecordStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandRecordStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band record {code} not found");
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

/// <summary>Functions of pay band record workflow.</summary>
public static class PayBandRecordWorkflow
{
    public static PayBandRecordManager NewPayBandRecordManager(IPayBandRecordStore store, IClock clock)
    {
        return new PayBandRecordManager(
            store,
            PayBandRecordRules.DefaultPayBandRecordValidator(),
            clock,
            new List<string>(),
            PayBandRecordEngine.DefaultPayBandRecordEngine());
    }
}
