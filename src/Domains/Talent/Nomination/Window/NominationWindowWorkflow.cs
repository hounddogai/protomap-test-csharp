namespace Acme.Workforce.Talent.Nomination.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves nomination windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationWindowService(INominationWindowStore Store, NominationWindowValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationWindowStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationWindowStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of nomination window workflow.</summary>
public static class NominationWindowWorkflow
{
    public static NominationWindowService NewNominationWindowService(INominationWindowStore store, IClock clock)
    {
        return new NominationWindowService(
            store,
            NominationWindowRules.DefaultNominationWindowValidator(),
            clock,
            new List<string>());
    }
}
