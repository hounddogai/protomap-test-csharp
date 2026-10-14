namespace Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient pay run ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientPayRunLedgerManager(ILenientPayRunLedgerStore Store, LenientPayRunLedgerValidator Validator, IClock Clock, List<string> Journal, LenientPayRunLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient pay run ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientPayRunLedgerStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient pay run ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientPayRunLedgerStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient pay run ledger {code} not found");
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

/// <summary>Functions of lenient pay run ledger workflow.</summary>
public static class LenientPayRunLedgerWorkflow
{
    public static LenientPayRunLedgerManager NewLenientPayRunLedgerManager(ILenientPayRunLedgerStore store, IClock clock, Registry flags)
    {
        return new LenientPayRunLedgerManager(
            store,
            LenientPayRunLedgerRules.DefaultLenientPayRunLedgerValidator(),
            clock,
            new List<string>(),
            LenientPayRunLedgerEngine.DefaultLenientPayRunLedgerEngine(flags));
    }
}
