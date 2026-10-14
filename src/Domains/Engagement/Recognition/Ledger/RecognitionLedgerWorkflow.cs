namespace Acme.Workforce.Engagement.Recognition.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves recognition ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionLedgerDesk(IRecognitionLedgerStore Store, RecognitionLedgerValidator Validator, IClock Clock, List<string> Journal, RecognitionLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionLedgerStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionLedgerStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition ledger {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of recognition ledger workflow.</summary>
public static class RecognitionLedgerWorkflow
{
    public static RecognitionLedgerDesk NewRecognitionLedgerDesk(IRecognitionLedgerStore store, IClock clock, Registry flags)
    {
        return new RecognitionLedgerDesk(
            store,
            RecognitionLedgerRules.DefaultRecognitionLedgerValidator(),
            clock,
            new List<string>(),
            RecognitionLedgerEngine.DefaultRecognitionLedgerEngine(flags));
    }
}
