namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient span ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientSpanLedgerService(ILenientSpanLedgerStore Store, LenientSpanLedgerValidator Validator, IClock Clock, List<string> Journal, LenientSpanLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient span ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientSpanLedgerStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient span ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientSpanLedgerStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient span ledger {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of lenient span ledger workflow.</summary>
public static class LenientSpanLedgerWorkflow
{
    public static LenientSpanLedgerService NewLenientSpanLedgerService(ILenientSpanLedgerStore store, IClock clock, Registry flags)
    {
        return new LenientSpanLedgerService(
            store,
            LenientSpanLedgerRules.DefaultLenientSpanLedgerValidator(),
            clock,
            new List<string>(),
            LenientSpanLedgerEngine.DefaultLenientSpanLedgerEngine(flags));
    }
}
