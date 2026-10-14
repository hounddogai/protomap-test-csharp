namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pay band proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandProposalService(IPayBandProposalStore Store, PayBandProposalValidator Validator, IClock Clock, List<string> Journal, PayBandProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandProposalStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandProposalStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band proposal {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of pay band proposal workflow.</summary>
public static class PayBandProposalWorkflow
{
    public static PayBandProposalService NewPayBandProposalService(IPayBandProposalStore store, IClock clock, Registry flags)
    {
        return new PayBandProposalService(
            store,
            PayBandProposalRules.DefaultPayBandProposalValidator(),
            clock,
            new List<string>(),
            PayBandProposalEngine.DefaultPayBandProposalEngine(flags));
    }
}
