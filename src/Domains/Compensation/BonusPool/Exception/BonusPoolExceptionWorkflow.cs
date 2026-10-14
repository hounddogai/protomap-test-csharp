namespace Acme.Workforce.Compensation.BonusPool.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves bonus pool exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolExceptionService(IBonusPoolExceptionStore Store, BonusPoolExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolExceptionStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolExceptionStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of bonus pool exception workflow.</summary>
public static class BonusPoolExceptionWorkflow
{
    public static BonusPoolExceptionService NewBonusPoolExceptionService(IBonusPoolExceptionStore store, IClock clock)
    {
        return new BonusPoolExceptionService(
            store,
            BonusPoolExceptionRules.DefaultBonusPoolExceptionValidator(),
            clock,
            new List<string>());
    }
}
