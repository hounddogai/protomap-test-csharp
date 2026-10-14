namespace Acme.Workforce.Compensation.Adjustment.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal adjustment proposals through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalAdjustmentProposalService(ISeasonalAdjustmentProposalStore Store, SeasonalAdjustmentProposalValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal adjustment proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalAdjustmentProposalStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal adjustment proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalAdjustmentProposalStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of seasonal adjustment proposal workflow.</summary>
public static class SeasonalAdjustmentProposalWorkflow
{
    public static SeasonalAdjustmentProposalService NewSeasonalAdjustmentProposalService(ISeasonalAdjustmentProposalStore store, IClock clock)
    {
        return new SeasonalAdjustmentProposalService(
            store,
            SeasonalAdjustmentProposalRules.DefaultSeasonalAdjustmentProposalValidator(),
            clock,
            new List<string>());
    }
}
