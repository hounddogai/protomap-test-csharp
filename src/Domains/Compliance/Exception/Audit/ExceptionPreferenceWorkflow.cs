namespace Acme.Workforce.Compliance.Exception.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves exception preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionPreferenceDesk(IExceptionPreferenceStore Store, ExceptionPreferenceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionPreferenceStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionPreferenceStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of exception preference workflow.</summary>
public static class ExceptionPreferenceWorkflow
{
    public static ExceptionPreferenceDesk NewExceptionPreferenceDesk(IExceptionPreferenceStore store, IClock clock)
    {
        return new ExceptionPreferenceDesk(
            store,
            ExceptionPreferenceRules.DefaultExceptionPreferenceValidator(),
            clock,
            new List<string>());
    }
}
