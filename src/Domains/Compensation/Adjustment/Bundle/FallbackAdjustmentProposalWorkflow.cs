namespace Acme.Workforce.Compensation.Adjustment.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback adjustment proposals through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackAdjustmentProposalDesk(IFallbackAdjustmentProposalStore Store, FallbackAdjustmentProposalValidator Validator, IClock Clock, List<string> Journal, FallbackAdjustmentProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback adjustment proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackAdjustmentProposalStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback adjustment proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackAdjustmentProposalStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback adjustment proposal {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of fallback adjustment proposal workflow.</summary>
public static class FallbackAdjustmentProposalWorkflow
{
    public static FallbackAdjustmentProposalDesk NewFallbackAdjustmentProposalDesk(IFallbackAdjustmentProposalStore store, IClock clock)
    {
        return new FallbackAdjustmentProposalDesk(
            store,
            FallbackAdjustmentProposalRules.DefaultFallbackAdjustmentProposalValidator(),
            clock,
            new List<string>(),
            FallbackAdjustmentProposalEngine.DefaultFallbackAdjustmentProposalEngine());
    }
}
