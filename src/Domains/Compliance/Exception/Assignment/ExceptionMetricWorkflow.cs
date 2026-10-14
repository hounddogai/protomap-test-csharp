namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves exception metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionMetricCoordinator(IExceptionMetricStore Store, ExceptionMetricValidator Validator, IClock Clock, List<string> Journal, ExceptionMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionMetricStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionMetricStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception metric {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of exception metric workflow.</summary>
public static class ExceptionMetricWorkflow
{
    public static ExceptionMetricCoordinator NewExceptionMetricCoordinator(IExceptionMetricStore store, IClock clock)
    {
        return new ExceptionMetricCoordinator(
            store,
            ExceptionMetricRules.DefaultExceptionMetricValidator(),
            clock,
            new List<string>(),
            ExceptionMetricEngine.DefaultExceptionMetricEngine());
    }
}
