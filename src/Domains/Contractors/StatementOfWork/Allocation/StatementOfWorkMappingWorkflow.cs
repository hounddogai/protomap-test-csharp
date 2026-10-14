namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves statement of work mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record StatementOfWorkMappingWorkflow(IStatementOfWorkMappingStore Store, StatementOfWorkMappingValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkMappingEngine Engine)
{
    public static StatementOfWorkMappingWorkflow NewStatementOfWorkMappingWorkflow(IStatementOfWorkMappingStore store, IClock clock)
    {
        return new StatementOfWorkMappingWorkflow(
            store,
            StatementOfWorkMappingRules.DefaultStatementOfWorkMappingValidator(),
            clock,
            new List<string>(),
            StatementOfWorkMappingEngine.DefaultStatementOfWorkMappingEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkMappingStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkMappingStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work mapping {code} not found");
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
