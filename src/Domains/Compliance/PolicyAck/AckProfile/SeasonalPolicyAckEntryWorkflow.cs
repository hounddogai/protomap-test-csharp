namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal policy ack entries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalPolicyAckEntryService(ISeasonalPolicyAckEntryStore Store, SeasonalPolicyAckEntryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal policy ack entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalPolicyAckEntryStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal policy ack entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalPolicyAckEntryStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of seasonal policy ack entry workflow.</summary>
public static class SeasonalPolicyAckEntryWorkflow
{
    public static SeasonalPolicyAckEntryService NewSeasonalPolicyAckEntryService(ISeasonalPolicyAckEntryStore store, IClock clock)
    {
        return new SeasonalPolicyAckEntryService(
            store,
            SeasonalPolicyAckEntryRules.DefaultSeasonalPolicyAckEntryValidator(),
            clock,
            new List<string>());
    }
}
