namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default pay run ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultPayRunLedgerWorkflow(IDefaultPayRunLedgerStore Store, DefaultPayRunLedgerValidator Validator, IClock Clock, List<string> Journal, DefaultPayRunLedgerEngine Engine)
{
    public static DefaultPayRunLedgerWorkflow NewDefaultPayRunLedgerWorkflow(IDefaultPayRunLedgerStore store, IClock clock, Registry flags)
    {
        return new DefaultPayRunLedgerWorkflow(
            store,
            DefaultPayRunLedgerRules.DefaultDefaultPayRunLedgerValidator(),
            clock,
            new List<string>(),
            DefaultPayRunLedgerEngine.DefaultDefaultPayRunLedgerEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default pay run ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultPayRunLedgerStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default pay run ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultPayRunLedgerStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default pay run ledger {code} not found");
        }
        var result = Engine.Compute(item);
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
