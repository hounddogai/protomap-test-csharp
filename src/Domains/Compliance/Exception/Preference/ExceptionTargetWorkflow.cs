namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves exception targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionTargetDesk(IExceptionTargetStore Store, ExceptionTargetValidator Validator, IClock Clock, List<string> Journal, ExceptionTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionTargetStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionTargetStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception target {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of exception target workflow.</summary>
public static class ExceptionTargetWorkflow
{
    public static ExceptionTargetDesk NewExceptionTargetDesk(IExceptionTargetStore store, IClock clock, Registry flags)
    {
        return new ExceptionTargetDesk(
            store,
            ExceptionTargetRules.DefaultExceptionTargetValidator(),
            clock,
            new List<string>(),
            ExceptionTargetEngine.DefaultExceptionTargetEngine(flags));
    }
}
