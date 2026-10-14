namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves attestation policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationPolicyService(IAttestationPolicyStore Store, AttestationPolicyValidator Validator, IClock Clock, List<string> Journal, AttestationPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationPolicyStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationPolicyStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation policy {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of attestation policy workflow.</summary>
public static class AttestationPolicyWorkflow
{
    public static AttestationPolicyService NewAttestationPolicyService(IAttestationPolicyStore store, IClock clock)
    {
        return new AttestationPolicyService(
            store,
            AttestationPolicyRules.DefaultAttestationPolicyValidator(),
            clock,
            new List<string>(),
            AttestationPolicyEngine.DefaultAttestationPolicyEngine());
    }
}
