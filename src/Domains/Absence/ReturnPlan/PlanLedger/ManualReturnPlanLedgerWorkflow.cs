namespace Acme.Workforce.Absence.ReturnPlan.PlanLedger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual return plan ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualReturnPlanLedgerWorkflow(IManualReturnPlanLedgerStore Store, ManualReturnPlanLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    public static ManualReturnPlanLedgerWorkflow NewManualReturnPlanLedgerWorkflow(IManualReturnPlanLedgerStore store, IClock clock)
    {
        return new ManualReturnPlanLedgerWorkflow(
            store,
            ManualReturnPlanLedgerRules.DefaultManualReturnPlanLedgerValidator(),
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
            throw new InvalidOperationException($"manual return plan ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualReturnPlanLedgerStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual return plan ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualReturnPlanLedgerStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
