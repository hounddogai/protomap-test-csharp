namespace Acme.Workforce.Payrollops.PayRun.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled pay run cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledPayRunCycleWorkflow(IScheduledPayRunCycleStore Store, ScheduledPayRunCycleValidator Validator, IClock Clock, List<string> Journal, ScheduledPayRunCycleEngine Engine)
{
    public static ScheduledPayRunCycleWorkflow NewScheduledPayRunCycleWorkflow(IScheduledPayRunCycleStore store, IClock clock, Registry flags)
    {
        return new ScheduledPayRunCycleWorkflow(
            store,
            ScheduledPayRunCycleRules.DefaultScheduledPayRunCycleValidator(),
            clock,
            new List<string>(),
            ScheduledPayRunCycleEngine.DefaultScheduledPayRunCycleEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay run cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPayRunCycleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay run cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPayRunCycleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay run cycle {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.CreatedAt > cutoff)
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
