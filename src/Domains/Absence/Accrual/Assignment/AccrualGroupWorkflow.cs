namespace Acme.Workforce.Absence.Accrual.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves accrual groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualGroupService(IAccrualGroupStore Store, AccrualGroupValidator Validator, IClock Clock, List<string> Journal, AccrualGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualGroupStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualGroupStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual group {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of accrual group workflow.</summary>
public static class AccrualGroupWorkflow
{
    public static AccrualGroupService NewAccrualGroupService(IAccrualGroupStore store, IClock clock, Registry flags)
    {
        return new AccrualGroupService(
            store,
            AccrualGroupRules.DefaultAccrualGroupValidator(),
            clock,
            new List<string>(),
            AccrualGroupEngine.DefaultAccrualGroupEngine(flags));
    }
}
