namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered feedback proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredFeedbackProposalCoordinator(ITieredFeedbackProposalStore Store, TieredFeedbackProposalValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered feedback proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredFeedbackProposalStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered feedback proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredFeedbackProposalStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of tiered feedback proposal workflow.</summary>
public static class TieredFeedbackProposalWorkflow
{
    public static TieredFeedbackProposalCoordinator NewTieredFeedbackProposalCoordinator(ITieredFeedbackProposalStore store, IClock clock)
    {
        return new TieredFeedbackProposalCoordinator(
            store,
            TieredFeedbackProposalRules.DefaultTieredFeedbackProposalValidator(),
            clock,
            new List<string>());
    }
}
