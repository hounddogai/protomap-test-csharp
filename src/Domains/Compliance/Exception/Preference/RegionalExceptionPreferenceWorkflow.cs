namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional exception preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalExceptionPreferenceService(IRegionalExceptionPreferenceStore Store, RegionalExceptionPreferenceValidator Validator, IClock Clock, List<string> Journal, RegionalExceptionPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional exception preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalExceptionPreferenceStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional exception preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalExceptionPreferenceStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional exception preference {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of regional exception preference workflow.</summary>
public static class RegionalExceptionPreferenceWorkflow
{
    public static RegionalExceptionPreferenceService NewRegionalExceptionPreferenceService(IRegionalExceptionPreferenceStore store, IClock clock, Registry flags)
    {
        return new RegionalExceptionPreferenceService(
            store,
            RegionalExceptionPreferenceRules.DefaultRegionalExceptionPreferenceValidator(),
            clock,
            new List<string>(),
            RegionalExceptionPreferenceEngine.DefaultRegionalExceptionPreferenceEngine(flags));
    }
}
