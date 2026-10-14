namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves balance agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceAgreementDesk(IBalanceAgreementStore Store, BalanceAgreementValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceAgreementStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceAgreementStatus.Closed);
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

/// <summary>Functions of balance agreement workflow.</summary>
public static class BalanceAgreementWorkflow
{
    public static BalanceAgreementDesk NewBalanceAgreementDesk(IBalanceAgreementStore store, IClock clock)
    {
        return new BalanceAgreementDesk(
            store,
            BalanceAgreementRules.DefaultBalanceAgreementValidator(),
            clock,
            new List<string>());
    }
}
