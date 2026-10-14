namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual statement of work profiles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualStatementOfWorkProfileManager(IManualStatementOfWorkProfileStore Store, ManualStatementOfWorkProfileValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual statement of work profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualStatementOfWorkProfileStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual statement of work profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualStatementOfWorkProfileStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of manual statement of work profile workflow.</summary>
public static class ManualStatementOfWorkProfileWorkflow
{
    public static ManualStatementOfWorkProfileManager NewManualStatementOfWorkProfileManager(IManualStatementOfWorkProfileStore store, IClock clock)
    {
        return new ManualStatementOfWorkProfileManager(
            store,
            ManualStatementOfWorkProfileRules.DefaultManualStatementOfWorkProfileValidator(),
            clock,
            new List<string>());
    }
}
