namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves attestation ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationLedgerDesk(IAttestationLedgerStore Store, AttestationLedgerValidator Validator, IClock Clock, List<string> Journal, AttestationLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationLedgerStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationLedgerStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation ledger {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of attestation ledger workflow.</summary>
public static class AttestationLedgerWorkflow
{
    public static AttestationLedgerDesk NewAttestationLedgerDesk(IAttestationLedgerStore store, IClock clock)
    {
        return new AttestationLedgerDesk(
            store,
            AttestationLedgerRules.DefaultAttestationLedgerValidator(),
            clock,
            new List<string>(),
            AttestationLedgerEngine.DefaultAttestationLedgerEngine());
    }
}
