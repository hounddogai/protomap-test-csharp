namespace Acme.Workforce.Contractors.StatementOfWork.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves statement of work rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record StatementOfWorkRuleDesk(IStatementOfWorkRuleStore Store, StatementOfWorkRuleValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkRuleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkRuleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work rule {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of statement of work rule workflow.</summary>
public static class StatementOfWorkRuleWorkflow
{
    public static StatementOfWorkRuleDesk NewStatementOfWorkRuleDesk(IStatementOfWorkRuleStore store, IClock clock, Registry flags)
    {
        return new StatementOfWorkRuleDesk(
            store,
            StatementOfWorkRuleRules.DefaultStatementOfWorkRuleValidator(),
            clock,
            new List<string>(),
            StatementOfWorkRuleEngine.DefaultStatementOfWorkRuleEngine(flags));
    }
}
