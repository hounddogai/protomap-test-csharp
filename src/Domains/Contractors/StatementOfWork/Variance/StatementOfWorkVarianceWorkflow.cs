namespace Acme.Workforce.Contractors.StatementOfWork.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves statement of work variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StatementOfWorkVarianceWorkflow(IStatementOfWorkVarianceStore Store, StatementOfWorkVarianceValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkVarianceEngine Engine)
{
    public static StatementOfWorkVarianceWorkflow NewStatementOfWorkVarianceWorkflow(IStatementOfWorkVarianceStore store, IClock clock, Registry flags)
    {
        return new StatementOfWorkVarianceWorkflow(
            store,
            StatementOfWorkVarianceRules.DefaultStatementOfWorkVarianceValidator(),
            clock,
            new List<string>(),
            StatementOfWorkVarianceEngine.DefaultStatementOfWorkVarianceEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkVarianceStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkVarianceStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work variance {code} not found");
        }
        var result = Engine.Price(item);
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
