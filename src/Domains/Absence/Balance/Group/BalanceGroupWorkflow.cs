namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves balance groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceGroupCoordinator(IBalanceGroupStore Store, BalanceGroupValidator Validator, IClock Clock, List<string> Journal, BalanceGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceGroupStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceGroupStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance group {code} not found");
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

/// <summary>Functions of balance group workflow.</summary>
public static class BalanceGroupWorkflow
{
    public static BalanceGroupCoordinator NewBalanceGroupCoordinator(IBalanceGroupStore store, IClock clock, Registry flags)
    {
        return new BalanceGroupCoordinator(
            store,
            BalanceGroupRules.DefaultBalanceGroupValidator(),
            clock,
            new List<string>(),
            BalanceGroupEngine.DefaultBalanceGroupEngine(flags));
    }
}
