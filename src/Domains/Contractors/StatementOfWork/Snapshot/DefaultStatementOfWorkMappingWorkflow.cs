namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default statement of work mappings through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultStatementOfWorkMappingWorkflow(IDefaultStatementOfWorkMappingStore Store, DefaultStatementOfWorkMappingValidator Validator, IClock Clock, List<string> Journal, DefaultStatementOfWorkMappingEngine Engine)
{
    public static DefaultStatementOfWorkMappingWorkflow NewDefaultStatementOfWorkMappingWorkflow(IDefaultStatementOfWorkMappingStore store, IClock clock, Registry flags)
    {
        return new DefaultStatementOfWorkMappingWorkflow(
            store,
            DefaultStatementOfWorkMappingRules.DefaultDefaultStatementOfWorkMappingValidator(),
            clock,
            new List<string>(),
            DefaultStatementOfWorkMappingEngine.DefaultDefaultStatementOfWorkMappingEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default statement of work mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultStatementOfWorkMappingStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default statement of work mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultStatementOfWorkMappingStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default statement of work mapping {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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
