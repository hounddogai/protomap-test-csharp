namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict nomination profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictNominationProfileWorkflow(IStrictNominationProfileStore Store, StrictNominationProfileValidator Validator, IClock Clock, List<string> Journal)
{
    public static StrictNominationProfileWorkflow NewStrictNominationProfileWorkflow(IStrictNominationProfileStore store, IClock clock)
    {
        return new StrictNominationProfileWorkflow(
            store,
            StrictNominationProfileRules.DefaultStrictNominationProfileValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict nomination profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictNominationProfileStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict nomination profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictNominationProfileStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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
