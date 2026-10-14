namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves exception summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionSummaryManager(IExceptionSummaryStore Store, ExceptionSummaryValidator Validator, IClock Clock, List<string> Journal, ExceptionSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionSummaryStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionSummaryStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception summary {code} not found");
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

/// <summary>Functions of exception summary workflow.</summary>
public static class ExceptionSummaryWorkflow
{
    public static ExceptionSummaryManager NewExceptionSummaryManager(IExceptionSummaryStore store, IClock clock)
    {
        return new ExceptionSummaryManager(
            store,
            ExceptionSummaryRules.DefaultExceptionSummaryValidator(),
            clock,
            new List<string>(),
            ExceptionSummaryEngine.DefaultExceptionSummaryEngine());
    }
}
