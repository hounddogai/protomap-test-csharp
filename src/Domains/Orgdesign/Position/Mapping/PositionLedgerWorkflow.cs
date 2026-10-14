namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionLedgerWorkflow(IPositionLedgerStore Store, PositionLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    public static PositionLedgerWorkflow NewPositionLedgerWorkflow(IPositionLedgerStore store, IClock clock)
    {
        return new PositionLedgerWorkflow(
            store,
            PositionLedgerRules.DefaultPositionLedgerValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionLedgerStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionLedgerStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
