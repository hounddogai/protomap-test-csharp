namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual bonus pool proposals through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualBonusPoolProposalCoordinator(IManualBonusPoolProposalStore Store, ManualBonusPoolProposalValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual bonus pool proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualBonusPoolProposalStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual bonus pool proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualBonusPoolProposalStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of manual bonus pool proposal workflow.</summary>
public static class ManualBonusPoolProposalWorkflow
{
    public static ManualBonusPoolProposalCoordinator NewManualBonusPoolProposalCoordinator(IManualBonusPoolProposalStore store, IClock clock)
    {
        return new ManualBonusPoolProposalCoordinator(
            store,
            ManualBonusPoolProposalRules.DefaultManualBonusPoolProposalValidator(),
            clock,
            new List<string>());
    }
}
