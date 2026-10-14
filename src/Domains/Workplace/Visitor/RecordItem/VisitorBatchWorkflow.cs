namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves visitor batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorBatchCoordinator(IVisitorBatchStore Store, VisitorBatchValidator Validator, IClock Clock, List<string> Journal, VisitorBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorBatchStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorBatchStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor batch {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of visitor batch workflow.</summary>
public static class VisitorBatchWorkflow
{
    public static VisitorBatchCoordinator NewVisitorBatchCoordinator(IVisitorBatchStore store, IClock clock)
    {
        return new VisitorBatchCoordinator(
            store,
            VisitorBatchRules.DefaultVisitorBatchValidator(),
            clock,
            new List<string>(),
            VisitorBatchEngine.DefaultVisitorBatchEngine());
    }
}
