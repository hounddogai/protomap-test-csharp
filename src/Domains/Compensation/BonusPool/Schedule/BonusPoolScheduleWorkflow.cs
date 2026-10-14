namespace Acme.Workforce.Compensation.BonusPool.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves bonus pool schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolScheduleWorkflow(IBonusPoolScheduleStore Store, BonusPoolScheduleValidator Validator, IClock Clock, List<string> Journal, BonusPoolScheduleEngine Engine)
{
    public static BonusPoolScheduleWorkflow NewBonusPoolScheduleWorkflow(IBonusPoolScheduleStore store, IClock clock, Registry flags)
    {
        return new BonusPoolScheduleWorkflow(
            store,
            BonusPoolScheduleRules.DefaultBonusPoolScheduleValidator(),
            clock,
            new List<string>(),
            BonusPoolScheduleEngine.DefaultBonusPoolScheduleEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolScheduleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolScheduleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool schedule {code} not found");
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
