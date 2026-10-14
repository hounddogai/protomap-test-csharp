namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional feedback proposals through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalFeedbackProposalManager(IRegionalFeedbackProposalStore Store, RegionalFeedbackProposalValidator Validator, IClock Clock, List<string> Journal, RegionalFeedbackProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional feedback proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalFeedbackProposalStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional feedback proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalFeedbackProposalStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional feedback proposal {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of regional feedback proposal workflow.</summary>
public static class RegionalFeedbackProposalWorkflow
{
    public static RegionalFeedbackProposalManager NewRegionalFeedbackProposalManager(IRegionalFeedbackProposalStore store, IClock clock)
    {
        return new RegionalFeedbackProposalManager(
            store,
            RegionalFeedbackProposalRules.DefaultRegionalFeedbackProposalValidator(),
            clock,
            new List<string>(),
            RegionalFeedbackProposalEngine.DefaultRegionalFeedbackProposalEngine());
    }
}
