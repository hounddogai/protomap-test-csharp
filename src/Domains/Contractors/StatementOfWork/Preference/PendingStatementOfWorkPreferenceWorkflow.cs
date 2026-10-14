namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending statement of work preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingStatementOfWorkPreferenceDesk(IPendingStatementOfWorkPreferenceStore Store, PendingStatementOfWorkPreferenceValidator Validator, IClock Clock, List<string> Journal, PendingStatementOfWorkPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending statement of work preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingStatementOfWorkPreferenceStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending statement of work preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingStatementOfWorkPreferenceStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending statement of work preference {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of pending statement of work preference workflow.</summary>
public static class PendingStatementOfWorkPreferenceWorkflow
{
    public static PendingStatementOfWorkPreferenceDesk NewPendingStatementOfWorkPreferenceDesk(IPendingStatementOfWorkPreferenceStore store, IClock clock, Registry flags)
    {
        return new PendingStatementOfWorkPreferenceDesk(
            store,
            PendingStatementOfWorkPreferenceRules.DefaultPendingStatementOfWorkPreferenceValidator(),
            clock,
            new List<string>(),
            PendingStatementOfWorkPreferenceEngine.DefaultPendingStatementOfWorkPreferenceEngine(flags));
    }
}
