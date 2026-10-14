namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched warranty ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedWarrantyLedgerWorkflow(IBatchedWarrantyLedgerStore Store, BatchedWarrantyLedgerValidator Validator, IClock Clock, List<string> Journal, BatchedWarrantyLedgerEngine Engine)
{
    public static BatchedWarrantyLedgerWorkflow NewBatchedWarrantyLedgerWorkflow(IBatchedWarrantyLedgerStore store, IClock clock)
    {
        return new BatchedWarrantyLedgerWorkflow(
            store,
            BatchedWarrantyLedgerRules.DefaultBatchedWarrantyLedgerValidator(),
            clock,
            new List<string>(),
            BatchedWarrantyLedgerEngine.DefaultBatchedWarrantyLedgerEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched warranty ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedWarrantyLedgerStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched warranty ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedWarrantyLedgerStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched warranty ledger {code} not found");
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
