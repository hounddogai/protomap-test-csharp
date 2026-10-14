namespace Acme.Workforce.Contractors.StatementOfWork.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves statement of work groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record StatementOfWorkGroupWorkflow(IStatementOfWorkGroupStore Store, StatementOfWorkGroupValidator Validator, IClock Clock, List<string> Journal)
{
    public static StatementOfWorkGroupWorkflow NewStatementOfWorkGroupWorkflow(IStatementOfWorkGroupStore store, IClock clock)
    {
        return new StatementOfWorkGroupWorkflow(
            store,
            StatementOfWorkGroupRules.DefaultStatementOfWorkGroupValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkGroupStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkGroupStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
