namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves statement of work thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StatementOfWorkThresholdCoordinator(IStatementOfWorkThresholdStore Store, StatementOfWorkThresholdValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkThresholdStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkThresholdStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of statement of work threshold workflow.</summary>
public static class StatementOfWorkThresholdWorkflow
{
    public static StatementOfWorkThresholdCoordinator NewStatementOfWorkThresholdCoordinator(IStatementOfWorkThresholdStore store, IClock clock)
    {
        return new StatementOfWorkThresholdCoordinator(
            store,
            StatementOfWorkThresholdRules.DefaultStatementOfWorkThresholdValidator(),
            clock,
            new List<string>());
    }
}
