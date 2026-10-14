namespace Acme.Workforce.Contractors.StatementOfWork.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict statement of work metrics through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictStatementOfWorkMetricManager(IStrictStatementOfWorkMetricStore Store, StrictStatementOfWorkMetricValidator Validator, IClock Clock, List<string> Journal, StrictStatementOfWorkMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict statement of work metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictStatementOfWorkMetricStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict statement of work metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictStatementOfWorkMetricStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict statement of work metric {code} not found");
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

/// <summary>Functions of strict statement of work metric workflow.</summary>
public static class StrictStatementOfWorkMetricWorkflow
{
    public static StrictStatementOfWorkMetricManager NewStrictStatementOfWorkMetricManager(IStrictStatementOfWorkMetricStore store, IClock clock, Registry flags)
    {
        return new StrictStatementOfWorkMetricManager(
            store,
            StrictStatementOfWorkMetricRules.DefaultStrictStatementOfWorkMetricValidator(),
            clock,
            new List<string>(),
            StrictStatementOfWorkMetricEngine.DefaultStrictStatementOfWorkMetricEngine(flags));
    }
}
