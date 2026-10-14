namespace Acme.Workforce.Absence.Accrual.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves accrual rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualRuleManager(IAccrualRuleStore Store, AccrualRuleValidator Validator, IClock Clock, List<string> Journal, AccrualRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualRuleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualRuleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual rule {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of accrual rule workflow.</summary>
public static class AccrualRuleWorkflow
{
    public static AccrualRuleManager NewAccrualRuleManager(IAccrualRuleStore store, IClock clock, Registry flags)
    {
        return new AccrualRuleManager(
            store,
            AccrualRuleRules.DefaultAccrualRuleValidator(),
            clock,
            new List<string>(),
            AccrualRuleEngine.DefaultAccrualRuleEngine(flags));
    }
}
