namespace Acme.Workforce.Contractors.Rate.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy rate preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyRatePreferenceDesk(ILegacyRatePreferenceStore Store, LegacyRatePreferenceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy rate preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyRatePreferenceStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy rate preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyRatePreferenceStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of legacy rate preference workflow.</summary>
public static class LegacyRatePreferenceWorkflow
{
    public static LegacyRatePreferenceDesk NewLegacyRatePreferenceDesk(ILegacyRatePreferenceStore store, IClock clock)
    {
        return new LegacyRatePreferenceDesk(
            store,
            LegacyRatePreferenceRules.DefaultLegacyRatePreferenceValidator(),
            clock,
            new List<string>());
    }
}
