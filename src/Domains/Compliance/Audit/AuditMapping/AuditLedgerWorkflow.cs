namespace Acme.Workforce.Compliance.Audit.AuditMapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves audit ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditLedgerWorkflow(IAuditLedgerStore Store, AuditLedgerValidator Validator, IClock Clock, List<string> Journal, AuditLedgerEngine Engine)
{
    public static AuditLedgerWorkflow NewAuditLedgerWorkflow(IAuditLedgerStore store, IClock clock)
    {
        return new AuditLedgerWorkflow(
            store,
            AuditLedgerRules.DefaultAuditLedgerValidator(),
            clock,
            new List<string>(),
            AuditLedgerEngine.DefaultAuditLedgerEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditLedgerStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditLedgerStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit ledger {code} not found");
        }
        var result = Engine.Run(item);
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
