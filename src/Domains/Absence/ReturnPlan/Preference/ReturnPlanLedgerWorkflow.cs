namespace Acme.Workforce.Absence.ReturnPlan.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves return plan ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReturnPlanLedgerWorkflow(IReturnPlanLedgerStore Store, ReturnPlanLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    public static ReturnPlanLedgerWorkflow NewReturnPlanLedgerWorkflow(IReturnPlanLedgerStore store, IClock clock)
    {
        return new ReturnPlanLedgerWorkflow(
            store,
            ReturnPlanLedgerRules.DefaultReturnPlanLedgerValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReturnPlanLedgerStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReturnPlanLedgerStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
