namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global feedback proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalFeedbackProposalService(IGlobalFeedbackProposalStore Store, GlobalFeedbackProposalValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global feedback proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalFeedbackProposalStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global feedback proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalFeedbackProposalStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of global feedback proposal workflow.</summary>
public static class GlobalFeedbackProposalWorkflow
{
    public static GlobalFeedbackProposalService NewGlobalFeedbackProposalService(IGlobalFeedbackProposalStore store, IClock clock)
    {
        return new GlobalFeedbackProposalService(
            store,
            GlobalFeedbackProposalRules.DefaultGlobalFeedbackProposalValidator(),
            clock,
            new List<string>());
    }
}
