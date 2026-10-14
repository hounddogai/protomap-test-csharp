namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling feedback proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingFeedbackProposalManager(IRollingFeedbackProposalStore Store, RollingFeedbackProposalValidator Validator, IClock Clock, List<string> Journal, RollingFeedbackProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling feedback proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingFeedbackProposalStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling feedback proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingFeedbackProposalStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling feedback proposal {code} not found");
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

/// <summary>Functions of rolling feedback proposal workflow.</summary>
public static class RollingFeedbackProposalWorkflow
{
    public static RollingFeedbackProposalManager NewRollingFeedbackProposalManager(IRollingFeedbackProposalStore store, IClock clock, Registry flags)
    {
        return new RollingFeedbackProposalManager(
            store,
            RollingFeedbackProposalRules.DefaultRollingFeedbackProposalValidator(),
            clock,
            new List<string>(),
            RollingFeedbackProposalEngine.DefaultRollingFeedbackProposalEngine(flags));
    }
}
