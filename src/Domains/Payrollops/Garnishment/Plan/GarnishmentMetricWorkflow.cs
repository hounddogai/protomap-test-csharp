namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves garnishment metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record GarnishmentMetricDesk(IGarnishmentMetricStore Store, GarnishmentMetricValidator Validator, IClock Clock, List<string> Journal, GarnishmentMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GarnishmentMetricStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GarnishmentMetricStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment metric {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of garnishment metric workflow.</summary>
public static class GarnishmentMetricWorkflow
{
    public static GarnishmentMetricDesk NewGarnishmentMetricDesk(IGarnishmentMetricStore store, IClock clock)
    {
        return new GarnishmentMetricDesk(
            store,
            GarnishmentMetricRules.DefaultGarnishmentMetricValidator(),
            clock,
            new List<string>(),
            GarnishmentMetricEngine.DefaultGarnishmentMetricEngine());
    }
}
