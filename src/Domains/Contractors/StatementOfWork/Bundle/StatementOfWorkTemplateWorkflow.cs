namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves statement of work templates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StatementOfWorkTemplateDesk(IStatementOfWorkTemplateStore Store, StatementOfWorkTemplateValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkTemplateStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkTemplateStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work template {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of statement of work template workflow.</summary>
public static class StatementOfWorkTemplateWorkflow
{
    public static StatementOfWorkTemplateDesk NewStatementOfWorkTemplateDesk(IStatementOfWorkTemplateStore store, IClock clock, Registry flags)
    {
        return new StatementOfWorkTemplateDesk(
            store,
            StatementOfWorkTemplateRules.DefaultStatementOfWorkTemplateValidator(),
            clock,
            new List<string>(),
            StatementOfWorkTemplateEngine.DefaultStatementOfWorkTemplateEngine(flags));
    }
}
