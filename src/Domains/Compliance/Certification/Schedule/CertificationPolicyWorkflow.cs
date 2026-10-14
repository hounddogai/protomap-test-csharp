namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves certification policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationPolicyService(ICertificationPolicyStore Store, CertificationPolicyValidator Validator, IClock Clock, List<string> Journal, CertificationPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationPolicyStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationPolicyStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification policy {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of certification policy workflow.</summary>
public static class CertificationPolicyWorkflow
{
    public static CertificationPolicyService NewCertificationPolicyService(ICertificationPolicyStore store, IClock clock, Registry flags)
    {
        return new CertificationPolicyService(
            store,
            CertificationPolicyRules.DefaultCertificationPolicyValidator(),
            clock,
            new List<string>(),
            CertificationPolicyEngine.DefaultCertificationPolicyEngine(flags));
    }
}
