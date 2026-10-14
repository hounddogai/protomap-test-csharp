namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pool schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolScheduleCoordinator(IPoolScheduleStore Store, PoolScheduleValidator Validator, IClock Clock, List<string> Journal, PoolScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolScheduleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolScheduleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool schedule {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of pool schedule workflow.</summary>
public static class PoolScheduleWorkflow
{
    public static PoolScheduleCoordinator NewPoolScheduleCoordinator(IPoolScheduleStore store, IClock clock, Registry flags)
    {
        return new PoolScheduleCoordinator(
            store,
            PoolScheduleRules.DefaultPoolScheduleValidator(),
            clock,
            new List<string>(),
            PoolScheduleEngine.DefaultPoolScheduleEngine(flags));
    }
}
