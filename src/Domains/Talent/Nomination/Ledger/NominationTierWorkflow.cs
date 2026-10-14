namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves nomination tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationTierManager(INominationTierStore Store, NominationTierValidator Validator, IClock Clock, List<string> Journal, NominationTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationTierStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationTierStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination tier {code} not found");
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

/// <summary>Functions of nomination tier workflow.</summary>
public static class NominationTierWorkflow
{
    public static NominationTierManager NewNominationTierManager(INominationTierStore store, IClock clock, Registry flags)
    {
        return new NominationTierManager(
            store,
            NominationTierRules.DefaultNominationTierValidator(),
            clock,
            new List<string>(),
            NominationTierEngine.DefaultNominationTierEngine(flags));
    }
}
