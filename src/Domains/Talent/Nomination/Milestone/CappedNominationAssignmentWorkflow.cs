namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped nomination assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedNominationAssignmentService(ICappedNominationAssignmentStore Store, CappedNominationAssignmentValidator Validator, IClock Clock, List<string> Journal, CappedNominationAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped nomination assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedNominationAssignmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped nomination assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedNominationAssignmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped nomination assignment {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of capped nomination assignment workflow.</summary>
public static class CappedNominationAssignmentWorkflow
{
    public static CappedNominationAssignmentService NewCappedNominationAssignmentService(ICappedNominationAssignmentStore store, IClock clock)
    {
        return new CappedNominationAssignmentService(
            store,
            CappedNominationAssignmentRules.DefaultCappedNominationAssignmentValidator(),
            clock,
            new List<string>(),
            CappedNominationAssignmentEngine.DefaultCappedNominationAssignmentEngine());
    }
}
