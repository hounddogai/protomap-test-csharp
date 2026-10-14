namespace Acme.Workforce.Talent.Mobility.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves mobility preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record MobilityPreferenceDesk(IMobilityPreferenceStore Store, MobilityPreferenceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MobilityPreferenceStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MobilityPreferenceStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of mobility preference workflow.</summary>
public static class MobilityPreferenceWorkflow
{
    public static MobilityPreferenceDesk NewMobilityPreferenceDesk(IMobilityPreferenceStore store, IClock clock)
    {
        return new MobilityPreferenceDesk(
            store,
            MobilityPreferenceRules.DefaultMobilityPreferenceValidator(),
            clock,
            new List<string>());
    }
}
