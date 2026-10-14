namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves attestation plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationPlanWorkflow(IAttestationPlanStore Store, AttestationPlanValidator Validator, IClock Clock, List<string> Journal, AttestationPlanEngine Engine)
{
    public static AttestationPlanWorkflow NewAttestationPlanWorkflow(IAttestationPlanStore store, IClock clock)
    {
        return new AttestationPlanWorkflow(
            store,
            AttestationPlanRules.DefaultAttestationPlanValidator(),
            clock,
            new List<string>(),
            AttestationPlanEngine.DefaultAttestationPlanEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationPlanStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationPlanStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation plan {code} not found");
        }
        var result = Engine.Evaluate(item);
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
