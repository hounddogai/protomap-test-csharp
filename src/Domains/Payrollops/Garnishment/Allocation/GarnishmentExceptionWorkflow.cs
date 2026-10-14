namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves garnishment exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record GarnishmentExceptionManager(IGarnishmentExceptionStore Store, GarnishmentExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GarnishmentExceptionStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GarnishmentExceptionStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of garnishment exception workflow.</summary>
public static class GarnishmentExceptionWorkflow
{
    public static GarnishmentExceptionManager NewGarnishmentExceptionManager(IGarnishmentExceptionStore store, IClock clock)
    {
        return new GarnishmentExceptionManager(
            store,
            GarnishmentExceptionRules.DefaultGarnishmentExceptionValidator(),
            clock,
            new List<string>());
    }
}
