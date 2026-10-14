namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves policy schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyScheduleCoordinator(IPolicyScheduleStore Store, PolicyScheduleValidator Validator, IClock Clock, List<string> Journal, PolicyScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyScheduleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyScheduleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy schedule {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of policy schedule workflow.</summary>
public static class PolicyScheduleWorkflow
{
    public static PolicyScheduleCoordinator NewPolicyScheduleCoordinator(IPolicyScheduleStore store, IClock clock, Registry flags)
    {
        return new PolicyScheduleCoordinator(
            store,
            PolicyScheduleRules.DefaultPolicyScheduleValidator(),
            clock,
            new List<string>(),
            PolicyScheduleEngine.DefaultPolicyScheduleEngine(flags));
    }
}
