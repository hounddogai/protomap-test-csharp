namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves garnishment requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record GarnishmentRequestDesk(IGarnishmentRequestStore Store, GarnishmentRequestValidator Validator, IClock Clock, List<string> Journal, GarnishmentRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GarnishmentRequestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GarnishmentRequestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment request {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of garnishment request workflow.</summary>
public static class GarnishmentRequestWorkflow
{
    public static GarnishmentRequestDesk NewGarnishmentRequestDesk(IGarnishmentRequestStore store, IClock clock, Registry flags)
    {
        return new GarnishmentRequestDesk(
            store,
            GarnishmentRequestRules.DefaultGarnishmentRequestValidator(),
            clock,
            new List<string>(),
            GarnishmentRequestEngine.DefaultGarnishmentRequestEngine(flags));
    }
}
