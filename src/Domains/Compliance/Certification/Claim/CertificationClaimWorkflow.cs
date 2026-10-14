namespace Acme.Workforce.Compliance.Certification.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves certification claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationClaimService(ICertificationClaimStore Store, CertificationClaimValidator Validator, IClock Clock, List<string> Journal, CertificationClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationClaimStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationClaimStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification claim {code} not found");
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

/// <summary>Functions of certification claim workflow.</summary>
public static class CertificationClaimWorkflow
{
    public static CertificationClaimService NewCertificationClaimService(ICertificationClaimStore store, IClock clock, Registry flags)
    {
        return new CertificationClaimService(
            store,
            CertificationClaimRules.DefaultCertificationClaimValidator(),
            clock,
            new List<string>(),
            CertificationClaimEngine.DefaultCertificationClaimEngine(flags));
    }
}
