namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal leave request reviews through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalLeaveRequestReviewManager(ISeasonalLeaveRequestReviewStore Store, SeasonalLeaveRequestReviewValidator Validator, IClock Clock, List<string> Journal, SeasonalLeaveRequestReviewEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal leave request review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalLeaveRequestReviewStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal leave request review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalLeaveRequestReviewStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal leave request review {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of seasonal leave request review workflow.</summary>
public static class SeasonalLeaveRequestReviewWorkflow
{
    public static SeasonalLeaveRequestReviewManager NewSeasonalLeaveRequestReviewManager(ISeasonalLeaveRequestReviewStore store, IClock clock)
    {
        return new SeasonalLeaveRequestReviewManager(
            store,
            SeasonalLeaveRequestReviewRules.DefaultSeasonalLeaveRequestReviewValidator(),
            clock,
            new List<string>(),
            SeasonalLeaveRequestReviewEngine.DefaultSeasonalLeaveRequestReviewEngine());
    }
}
