namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay band policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandPolicyManager(IPayBandPolicyStore Store, PayBandPolicyValidator Validator, IClock Clock, List<string> Journal, PayBandPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandPolicyStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandPolicyStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band policy {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of pay band policy workflow.</summary>
public static class PayBandPolicyWorkflow
{
    public static PayBandPolicyManager NewPayBandPolicyManager(IPayBandPolicyStore store, IClock clock)
    {
        return new PayBandPolicyManager(
            store,
            PayBandPolicyRules.DefaultPayBandPolicyValidator(),
            clock,
            new List<string>(),
            PayBandPolicyEngine.DefaultPayBandPolicyEngine());
    }
}
