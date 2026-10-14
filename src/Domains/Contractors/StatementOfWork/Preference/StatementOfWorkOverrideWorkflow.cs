namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves statement of work overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StatementOfWorkOverrideService(IStatementOfWorkOverrideStore Store, StatementOfWorkOverrideValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkOverrideStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkOverrideStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work override {code} not found");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of statement of work override workflow.</summary>
public static class StatementOfWorkOverrideWorkflow
{
    public static StatementOfWorkOverrideService NewStatementOfWorkOverrideService(IStatementOfWorkOverrideStore store, IClock clock, Registry flags)
    {
        return new StatementOfWorkOverrideService(
            store,
            StatementOfWorkOverrideRules.DefaultStatementOfWorkOverrideValidator(),
            clock,
            new List<string>(),
            StatementOfWorkOverrideEngine.DefaultStatementOfWorkOverrideEngine(flags));
    }
}
