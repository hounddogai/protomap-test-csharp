namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal exception metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalExceptionMetricManager(ISeasonalExceptionMetricStore Store, SeasonalExceptionMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal exception metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalExceptionMetricStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal exception metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalExceptionMetricStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of seasonal exception metric workflow.</summary>
public static class SeasonalExceptionMetricWorkflow
{
    public static SeasonalExceptionMetricManager NewSeasonalExceptionMetricManager(ISeasonalExceptionMetricStore store, IClock clock)
    {
        return new SeasonalExceptionMetricManager(
            store,
            SeasonalExceptionMetricRules.DefaultSeasonalExceptionMetricValidator(),
            clock,
            new List<string>());
    }
}
