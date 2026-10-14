namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves statement of work assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StatementOfWorkAssignmentWorkflow(IStatementOfWorkAssignmentStore Store, StatementOfWorkAssignmentValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkAssignmentEngine Engine)
{
    public static StatementOfWorkAssignmentWorkflow NewStatementOfWorkAssignmentWorkflow(IStatementOfWorkAssignmentStore store, IClock clock, Registry flags)
    {
        return new StatementOfWorkAssignmentWorkflow(
            store,
            StatementOfWorkAssignmentRules.DefaultStatementOfWorkAssignmentValidator(),
            clock,
            new List<string>(),
            StatementOfWorkAssignmentEngine.DefaultStatementOfWorkAssignmentEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkAssignmentStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkAssignmentStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work assignment {code} not found");
        }
        var result = Engine.Price(item);
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
