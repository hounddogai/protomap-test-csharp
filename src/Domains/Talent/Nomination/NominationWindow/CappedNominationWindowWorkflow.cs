namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped nomination windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedNominationWindowService(ICappedNominationWindowStore Store, CappedNominationWindowValidator Validator, IClock Clock, List<string> Journal, CappedNominationWindowEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped nomination window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedNominationWindowStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped nomination window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedNominationWindowStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped nomination window {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of capped nomination window workflow.</summary>
public static class CappedNominationWindowWorkflow
{
    public static CappedNominationWindowService NewCappedNominationWindowService(ICappedNominationWindowStore store, IClock clock)
    {
        return new CappedNominationWindowService(
            store,
            CappedNominationWindowRules.DefaultCappedNominationWindowValidator(),
            clock,
            new List<string>(),
            CappedNominationWindowEngine.DefaultCappedNominationWindowEngine());
    }
}
