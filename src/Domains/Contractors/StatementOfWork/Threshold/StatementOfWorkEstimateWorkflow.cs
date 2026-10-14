namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves statement of work estimates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StatementOfWorkEstimateWorkflow(IStatementOfWorkEstimateStore Store, StatementOfWorkEstimateValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkEstimateEngine Engine)
{
    public static StatementOfWorkEstimateWorkflow NewStatementOfWorkEstimateWorkflow(IStatementOfWorkEstimateStore store, IClock clock, Registry flags)
    {
        return new StatementOfWorkEstimateWorkflow(
            store,
            StatementOfWorkEstimateRules.DefaultStatementOfWorkEstimateValidator(),
            clock,
            new List<string>(),
            StatementOfWorkEstimateEngine.DefaultStatementOfWorkEstimateEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkEstimateStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkEstimateStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work estimate {code} not found");
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
