namespace Acme.Workforce.Engagement.Feedback.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves feedback proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record FeedbackProposalService(IFeedbackProposalStore Store, FeedbackProposalValidator Validator, IClock Clock, List<string> Journal, FeedbackProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FeedbackProposalStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FeedbackProposalStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback proposal {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of feedback proposal workflow.</summary>
public static class FeedbackProposalWorkflow
{
    public static FeedbackProposalService NewFeedbackProposalService(IFeedbackProposalStore store, IClock clock, Registry flags)
    {
        return new FeedbackProposalService(
            store,
            FeedbackProposalRules.DefaultFeedbackProposalValidator(),
            clock,
            new List<string>(),
            FeedbackProposalEngine.DefaultFeedbackProposalEngine(flags));
    }
}
