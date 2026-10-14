namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves mobility drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record MobilityDraftCoordinator(IMobilityDraftStore Store, MobilityDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MobilityDraftStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MobilityDraftStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of mobility draft workflow.</summary>
public static class MobilityDraftWorkflow
{
    public static MobilityDraftCoordinator NewMobilityDraftCoordinator(IMobilityDraftStore store, IClock clock)
    {
        return new MobilityDraftCoordinator(
            store,
            MobilityDraftRules.DefaultMobilityDraftValidator(),
            clock,
            new List<string>());
    }
}
