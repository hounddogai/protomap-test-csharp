namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adjustment proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentProposalService(IAdjustmentProposalStore Store, AdjustmentProposalValidator Validator, IClock Clock, List<string> Journal, AdjustmentProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentProposalStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentProposalStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment proposal {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of adjustment proposal workflow.</summary>
public static class AdjustmentProposalWorkflow
{
    public static AdjustmentProposalService NewAdjustmentProposalService(IAdjustmentProposalStore store, IClock clock)
    {
        return new AdjustmentProposalService(
            store,
            AdjustmentProposalRules.DefaultAdjustmentProposalValidator(),
            clock,
            new List<string>(),
            AdjustmentProposalEngine.DefaultAdjustmentProposalEngine());
    }
}
