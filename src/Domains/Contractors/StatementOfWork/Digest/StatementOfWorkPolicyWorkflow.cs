namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves statement of work policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record StatementOfWorkPolicyDesk(IStatementOfWorkPolicyStore Store, StatementOfWorkPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkPolicyStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkPolicyStatus.Archived);
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

/// <summary>Functions of statement of work policy workflow.</summary>
public static class StatementOfWorkPolicyWorkflow
{
    public static StatementOfWorkPolicyDesk NewStatementOfWorkPolicyDesk(IStatementOfWorkPolicyStore store, IClock clock)
    {
        return new StatementOfWorkPolicyDesk(
            store,
            StatementOfWorkPolicyRules.DefaultStatementOfWorkPolicyValidator(),
            clock,
            new List<string>());
    }
}
