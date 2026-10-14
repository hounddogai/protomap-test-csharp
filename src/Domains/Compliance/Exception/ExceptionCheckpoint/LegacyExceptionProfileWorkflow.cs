namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy exception profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyExceptionProfileManager(ILegacyExceptionProfileStore Store, LegacyExceptionProfileValidator Validator, IClock Clock, List<string> Journal, LegacyExceptionProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy exception profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyExceptionProfileStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy exception profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyExceptionProfileStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy exception profile {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of legacy exception profile workflow.</summary>
public static class LegacyExceptionProfileWorkflow
{
    public static LegacyExceptionProfileManager NewLegacyExceptionProfileManager(ILegacyExceptionProfileStore store, IClock clock, Registry flags)
    {
        return new LegacyExceptionProfileManager(
            store,
            LegacyExceptionProfileRules.DefaultLegacyExceptionProfileValidator(),
            clock,
            new List<string>(),
            LegacyExceptionProfileEngine.DefaultLegacyExceptionProfileEngine(flags));
    }
}
