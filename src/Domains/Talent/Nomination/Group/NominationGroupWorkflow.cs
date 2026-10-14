namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves nomination groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationGroupCoordinator(INominationGroupStore Store, NominationGroupValidator Validator, IClock Clock, List<string> Journal, NominationGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationGroupStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationGroupStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination group {code} not found");
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

/// <summary>Functions of nomination group workflow.</summary>
public static class NominationGroupWorkflow
{
    public static NominationGroupCoordinator NewNominationGroupCoordinator(INominationGroupStore store, IClock clock)
    {
        return new NominationGroupCoordinator(
            store,
            NominationGroupRules.DefaultNominationGroupValidator(),
            clock,
            new List<string>(),
            NominationGroupEngine.DefaultNominationGroupEngine());
    }
}
