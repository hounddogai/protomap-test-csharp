namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves nomination entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationEntryService(INominationEntryStore Store, NominationEntryValidator Validator, IClock Clock, List<string> Journal, NominationEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationEntryStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationEntryStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination entry {code} not found");
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

/// <summary>Functions of nomination entry workflow.</summary>
public static class NominationEntryWorkflow
{
    public static NominationEntryService NewNominationEntryService(INominationEntryStore store, IClock clock, Registry flags)
    {
        return new NominationEntryService(
            store,
            NominationEntryRules.DefaultNominationEntryValidator(),
            clock,
            new List<string>(),
            NominationEntryEngine.DefaultNominationEntryEngine(flags));
    }
}
