namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves attestation groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationGroupWorkflow(IAttestationGroupStore Store, AttestationGroupValidator Validator, IClock Clock, List<string> Journal, AttestationGroupEngine Engine)
{
    public static AttestationGroupWorkflow NewAttestationGroupWorkflow(IAttestationGroupStore store, IClock clock, Registry flags)
    {
        return new AttestationGroupWorkflow(
            store,
            AttestationGroupRules.DefaultAttestationGroupValidator(),
            clock,
            new List<string>(),
            AttestationGroupEngine.DefaultAttestationGroupEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationGroupStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationGroupStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation group {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.EndsAt > cutoff)
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
