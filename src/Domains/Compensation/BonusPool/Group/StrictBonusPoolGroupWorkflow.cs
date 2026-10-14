namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict bonus pool groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictBonusPoolGroupManager(IStrictBonusPoolGroupStore Store, StrictBonusPoolGroupValidator Validator, IClock Clock, List<string> Journal, StrictBonusPoolGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict bonus pool group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictBonusPoolGroupStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict bonus pool group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictBonusPoolGroupStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict bonus pool group {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of strict bonus pool group workflow.</summary>
public static class StrictBonusPoolGroupWorkflow
{
    public static StrictBonusPoolGroupManager NewStrictBonusPoolGroupManager(IStrictBonusPoolGroupStore store, IClock clock, Registry flags)
    {
        return new StrictBonusPoolGroupManager(
            store,
            StrictBonusPoolGroupRules.DefaultStrictBonusPoolGroupValidator(),
            clock,
            new List<string>(),
            StrictBonusPoolGroupEngine.DefaultStrictBonusPoolGroupEngine(flags));
    }
}
