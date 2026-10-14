namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global balance schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalBalanceScheduleWorkflow(IGlobalBalanceScheduleStore Store, GlobalBalanceScheduleValidator Validator, IClock Clock, List<string> Journal, GlobalBalanceScheduleEngine Engine)
{
    public static GlobalBalanceScheduleWorkflow NewGlobalBalanceScheduleWorkflow(IGlobalBalanceScheduleStore store, IClock clock, Registry flags)
    {
        return new GlobalBalanceScheduleWorkflow(
            store,
            GlobalBalanceScheduleRules.DefaultGlobalBalanceScheduleValidator(),
            clock,
            new List<string>(),
            GlobalBalanceScheduleEngine.DefaultGlobalBalanceScheduleEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global balance schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalBalanceScheduleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global balance schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalBalanceScheduleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global balance schedule {code} not found");
        }
        var result = Engine.Evaluate(item);
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
