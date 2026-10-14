namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves nomination assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationAssignmentService(INominationAssignmentStore Store, NominationAssignmentValidator Validator, IClock Clock, List<string> Journal, NominationAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationAssignmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationAssignmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination assignment {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of nomination assignment workflow.</summary>
public static class NominationAssignmentWorkflow
{
    public static NominationAssignmentService NewNominationAssignmentService(INominationAssignmentStore store, IClock clock, Registry flags)
    {
        return new NominationAssignmentService(
            store,
            NominationAssignmentRules.DefaultNominationAssignmentValidator(),
            clock,
            new List<string>(),
            NominationAssignmentEngine.DefaultNominationAssignmentEngine(flags));
    }
}
