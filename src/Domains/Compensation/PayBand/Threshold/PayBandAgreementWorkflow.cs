namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pay band agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandAgreementDesk(IPayBandAgreementStore Store, PayBandAgreementValidator Validator, IClock Clock, List<string> Journal, PayBandAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandAgreementStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandAgreementStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band agreement {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of pay band agreement workflow.</summary>
public static class PayBandAgreementWorkflow
{
    public static PayBandAgreementDesk NewPayBandAgreementDesk(IPayBandAgreementStore store, IClock clock, Registry flags)
    {
        return new PayBandAgreementDesk(
            store,
            PayBandAgreementRules.DefaultPayBandAgreementValidator(),
            clock,
            new List<string>(),
            PayBandAgreementEngine.DefaultPayBandAgreementEngine(flags));
    }
}
