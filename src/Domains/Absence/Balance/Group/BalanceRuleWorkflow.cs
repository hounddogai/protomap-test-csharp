namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves balance rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceRuleService(IBalanceRuleStore Store, BalanceRuleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceRuleStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceRuleStatus.Retired);
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

/// <summary>Functions of balance rule workflow.</summary>
public static class BalanceRuleWorkflow
{
    public static BalanceRuleService NewBalanceRuleService(IBalanceRuleStore store, IClock clock)
    {
        return new BalanceRuleService(
            store,
            BalanceRuleRules.DefaultBalanceRuleValidator(),
            clock,
            new List<string>());
    }
}
