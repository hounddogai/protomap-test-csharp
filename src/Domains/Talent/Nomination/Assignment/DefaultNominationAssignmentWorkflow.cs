namespace Acme.Workforce.Talent.Nomination.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default nomination assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultNominationAssignmentManager(IDefaultNominationAssignmentStore Store, DefaultNominationAssignmentValidator Validator, IClock Clock, List<string> Journal, DefaultNominationAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default nomination assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultNominationAssignmentStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default nomination assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultNominationAssignmentStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default nomination assignment {code} not found");
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

/// <summary>Functions of default nomination assignment workflow.</summary>
public static class DefaultNominationAssignmentWorkflow
{
    public static DefaultNominationAssignmentManager NewDefaultNominationAssignmentManager(IDefaultNominationAssignmentStore store, IClock clock)
    {
        return new DefaultNominationAssignmentManager(
            store,
            DefaultNominationAssignmentRules.DefaultDefaultNominationAssignmentValidator(),
            clock,
            new List<string>(),
            DefaultNominationAssignmentEngine.DefaultDefaultNominationAssignmentEngine());
    }
}
