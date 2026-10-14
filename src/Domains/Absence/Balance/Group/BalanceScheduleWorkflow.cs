namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves balance schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceScheduleWorkflow(IBalanceScheduleStore Store, BalanceScheduleValidator Validator, IClock Clock, List<string> Journal)
{
    public static BalanceScheduleWorkflow NewBalanceScheduleWorkflow(IBalanceScheduleStore store, IClock clock)
    {
        return new BalanceScheduleWorkflow(
            store,
            BalanceScheduleRules.DefaultBalanceScheduleValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceScheduleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceScheduleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
