namespace Acme.Workforce.Compliance.Certification.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves certification ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationLedgerService(ICertificationLedgerStore Store, CertificationLedgerValidator Validator, IClock Clock, List<string> Journal, CertificationLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationLedgerStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationLedgerStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification ledger {code} not found");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of certification ledger workflow.</summary>
public static class CertificationLedgerWorkflow
{
    public static CertificationLedgerService NewCertificationLedgerService(ICertificationLedgerStore store, IClock clock)
    {
        return new CertificationLedgerService(
            store,
            CertificationLedgerRules.DefaultCertificationLedgerValidator(),
            clock,
            new List<string>(),
            CertificationLedgerEngine.DefaultCertificationLedgerEngine());
    }
}
