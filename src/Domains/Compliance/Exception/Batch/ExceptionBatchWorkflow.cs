namespace Acme.Workforce.Compliance.Exception.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves exception batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionBatchDesk(IExceptionBatchStore Store, ExceptionBatchValidator Validator, IClock Clock, List<string> Journal, ExceptionBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionBatchStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionBatchStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception batch {code} not found");
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

/// <summary>Functions of exception batch workflow.</summary>
public static class ExceptionBatchWorkflow
{
    public static ExceptionBatchDesk NewExceptionBatchDesk(IExceptionBatchStore store, IClock clock, Registry flags)
    {
        return new ExceptionBatchDesk(
            store,
            ExceptionBatchRules.DefaultExceptionBatchValidator(),
            clock,
            new List<string>(),
            ExceptionBatchEngine.DefaultExceptionBatchEngine(flags));
    }
}
