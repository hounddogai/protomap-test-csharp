namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves asset return ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnLedgerWorkflow(IAssetReturnLedgerStore Store, AssetReturnLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    public static AssetReturnLedgerWorkflow NewAssetReturnLedgerWorkflow(IAssetReturnLedgerStore store, IClock clock)
    {
        return new AssetReturnLedgerWorkflow(
            store,
            AssetReturnLedgerRules.DefaultAssetReturnLedgerValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnLedgerStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnLedgerStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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
