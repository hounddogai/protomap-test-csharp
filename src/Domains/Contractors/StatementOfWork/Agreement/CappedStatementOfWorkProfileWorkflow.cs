namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped statement of work profiles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedStatementOfWorkProfileCoordinator(ICappedStatementOfWorkProfileStore Store, CappedStatementOfWorkProfileValidator Validator, IClock Clock, List<string> Journal, CappedStatementOfWorkProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped statement of work profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedStatementOfWorkProfileStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped statement of work profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedStatementOfWorkProfileStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped statement of work profile {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of capped statement of work profile workflow.</summary>
public static class CappedStatementOfWorkProfileWorkflow
{
    public static CappedStatementOfWorkProfileCoordinator NewCappedStatementOfWorkProfileCoordinator(ICappedStatementOfWorkProfileStore store, IClock clock, Registry flags)
    {
        return new CappedStatementOfWorkProfileCoordinator(
            store,
            CappedStatementOfWorkProfileRules.DefaultCappedStatementOfWorkProfileValidator(),
            clock,
            new List<string>(),
            CappedStatementOfWorkProfileEngine.DefaultCappedStatementOfWorkProfileEngine(flags));
    }
}
