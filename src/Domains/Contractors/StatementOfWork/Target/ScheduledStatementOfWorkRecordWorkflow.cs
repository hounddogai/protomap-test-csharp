namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled statement of work records through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledStatementOfWorkRecordDesk(IScheduledStatementOfWorkRecordStore Store, ScheduledStatementOfWorkRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled statement of work record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledStatementOfWorkRecordStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled statement of work record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledStatementOfWorkRecordStatus.Archived);
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

/// <summary>Functions of scheduled statement of work record workflow.</summary>
public static class ScheduledStatementOfWorkRecordWorkflow
{
    public static ScheduledStatementOfWorkRecordDesk NewScheduledStatementOfWorkRecordDesk(IScheduledStatementOfWorkRecordStore store, IClock clock)
    {
        return new ScheduledStatementOfWorkRecordDesk(
            store,
            ScheduledStatementOfWorkRecordRules.DefaultScheduledStatementOfWorkRecordValidator(),
            clock,
            new List<string>());
    }
}
