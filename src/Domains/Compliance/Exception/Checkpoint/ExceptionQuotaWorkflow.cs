namespace Acme.Workforce.Compliance.Exception.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves exception quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionQuotaService(IExceptionQuotaStore Store, ExceptionQuotaValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionQuotaStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionQuotaStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of exception quota workflow.</summary>
public static class ExceptionQuotaWorkflow
{
    public static ExceptionQuotaService NewExceptionQuotaService(IExceptionQuotaStore store, IClock clock)
    {
        return new ExceptionQuotaService(
            store,
            ExceptionQuotaRules.DefaultExceptionQuotaValidator(),
            clock,
            new List<string>());
    }
}
