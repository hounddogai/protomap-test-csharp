namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional balance targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalBalanceTargetService(IRegionalBalanceTargetStore Store, RegionalBalanceTargetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional balance target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalBalanceTargetStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional balance target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalBalanceTargetStatus.Withdrawn);
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

/// <summary>Functions of regional balance target workflow.</summary>
public static class RegionalBalanceTargetWorkflow
{
    public static RegionalBalanceTargetService NewRegionalBalanceTargetService(IRegionalBalanceTargetStore store, IClock clock)
    {
        return new RegionalBalanceTargetService(
            store,
            RegionalBalanceTargetRules.DefaultRegionalBalanceTargetValidator(),
            clock,
            new List<string>());
    }
}
