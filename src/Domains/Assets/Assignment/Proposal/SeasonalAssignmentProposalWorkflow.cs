namespace Acme.Workforce.Assets.Assignment.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal assignment proposals through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalAssignmentProposalService(ISeasonalAssignmentProposalStore Store, SeasonalAssignmentProposalValidator Validator, IClock Clock, List<string> Journal, SeasonalAssignmentProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal assignment proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalAssignmentProposalStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal assignment proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalAssignmentProposalStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal assignment proposal {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of seasonal assignment proposal workflow.</summary>
public static class SeasonalAssignmentProposalWorkflow
{
    public static SeasonalAssignmentProposalService NewSeasonalAssignmentProposalService(ISeasonalAssignmentProposalStore store, IClock clock, Registry flags)
    {
        return new SeasonalAssignmentProposalService(
            store,
            SeasonalAssignmentProposalRules.DefaultSeasonalAssignmentProposalValidator(),
            clock,
            new List<string>(),
            SeasonalAssignmentProposalEngine.DefaultSeasonalAssignmentProposalEngine(flags));
    }
}
