namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves policy cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyCycleCoordinator(IPolicyCycleStore Store, PolicyCycleValidator Validator, IClock Clock, List<string> Journal, PolicyCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyCycleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyCycleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy cycle {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of policy cycle workflow.</summary>
public static class PolicyCycleWorkflow
{
    public static PolicyCycleCoordinator NewPolicyCycleCoordinator(IPolicyCycleStore store, IClock clock, Registry flags)
    {
        return new PolicyCycleCoordinator(
            store,
            PolicyCycleRules.DefaultPolicyCycleValidator(),
            clock,
            new List<string>(),
            PolicyCycleEngine.DefaultPolicyCycleEngine(flags));
    }
}
