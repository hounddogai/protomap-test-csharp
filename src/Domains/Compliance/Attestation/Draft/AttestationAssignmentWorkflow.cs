namespace Acme.Workforce.Compliance.Attestation.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves attestation assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationAssignmentWorkflow(IAttestationAssignmentStore Store, AttestationAssignmentValidator Validator, IClock Clock, List<string> Journal, AttestationAssignmentEngine Engine)
{
    public static AttestationAssignmentWorkflow NewAttestationAssignmentWorkflow(IAttestationAssignmentStore store, IClock clock)
    {
        return new AttestationAssignmentWorkflow(
            store,
            AttestationAssignmentRules.DefaultAttestationAssignmentValidator(),
            clock,
            new List<string>(),
            AttestationAssignmentEngine.DefaultAttestationAssignmentEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationAssignmentStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationAssignmentStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation assignment {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.CreatedAt > cutoff)
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
