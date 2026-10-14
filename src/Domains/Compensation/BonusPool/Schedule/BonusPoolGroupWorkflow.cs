namespace Acme.Workforce.Compensation.BonusPool.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves bonus pool groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolGroupWorkflow(IBonusPoolGroupStore Store, BonusPoolGroupValidator Validator, IClock Clock, List<string> Journal, BonusPoolGroupEngine Engine)
{
    public static BonusPoolGroupWorkflow NewBonusPoolGroupWorkflow(IBonusPoolGroupStore store, IClock clock, Registry flags)
    {
        return new BonusPoolGroupWorkflow(
            store,
            BonusPoolGroupRules.DefaultBonusPoolGroupValidator(),
            clock,
            new List<string>(),
            BonusPoolGroupEngine.DefaultBonusPoolGroupEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolGroupStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolGroupStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool group {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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
