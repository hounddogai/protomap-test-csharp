namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves leave request reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestReviewService(ILeaveRequestReviewStore Store, LeaveRequestReviewValidator Validator, IClock Clock, List<string> Journal, LeaveRequestReviewEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestReviewStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestReviewStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request review {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of leave request review workflow.</summary>
public static class LeaveRequestReviewWorkflow
{
    public static LeaveRequestReviewService NewLeaveRequestReviewService(ILeaveRequestReviewStore store, IClock clock)
    {
        return new LeaveRequestReviewService(
            store,
            LeaveRequestReviewRules.DefaultLeaveRequestReviewValidator(),
            clock,
            new List<string>(),
            LeaveRequestReviewEngine.DefaultLeaveRequestReviewEngine());
    }
}
