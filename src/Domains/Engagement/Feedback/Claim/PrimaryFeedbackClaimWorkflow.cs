namespace Acme.Workforce.Engagement.Feedback.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary feedback claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryFeedbackClaimDesk(IPrimaryFeedbackClaimStore Store, PrimaryFeedbackClaimValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary feedback claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryFeedbackClaimStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary feedback claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryFeedbackClaimStatus.Stopped);
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

/// <summary>Functions of primary feedback claim workflow.</summary>
public static class PrimaryFeedbackClaimWorkflow
{
    public static PrimaryFeedbackClaimDesk NewPrimaryFeedbackClaimDesk(IPrimaryFeedbackClaimStore store, IClock clock)
    {
        return new PrimaryFeedbackClaimDesk(
            store,
            PrimaryFeedbackClaimRules.DefaultPrimaryFeedbackClaimValidator(),
            clock,
            new List<string>());
    }
}
