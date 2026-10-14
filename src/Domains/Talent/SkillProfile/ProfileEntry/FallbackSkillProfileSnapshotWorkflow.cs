namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback skill profile snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackSkillProfileSnapshotCoordinator(IFallbackSkillProfileSnapshotStore Store, FallbackSkillProfileSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback skill profile snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackSkillProfileSnapshotStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback skill profile snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackSkillProfileSnapshotStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of fallback skill profile snapshot workflow.</summary>
public static class FallbackSkillProfileSnapshotWorkflow
{
    public static FallbackSkillProfileSnapshotCoordinator NewFallbackSkillProfileSnapshotCoordinator(IFallbackSkillProfileSnapshotStore store, IClock clock)
    {
        return new FallbackSkillProfileSnapshotCoordinator(
            store,
            FallbackSkillProfileSnapshotRules.DefaultFallbackSkillProfileSnapshotValidator(),
            clock,
            new List<string>());
    }
}
