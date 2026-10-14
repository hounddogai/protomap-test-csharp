namespace Acme.Workforce.Compliance.Audit.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal audit preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalAuditPreferenceManager(ISeasonalAuditPreferenceStore Store, SeasonalAuditPreferenceValidator Validator, IClock Clock, List<string> Journal, SeasonalAuditPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal audit preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalAuditPreferenceStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal audit preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalAuditPreferenceStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal audit preference {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of seasonal audit preference workflow.</summary>
public static class SeasonalAuditPreferenceWorkflow
{
    public static SeasonalAuditPreferenceManager NewSeasonalAuditPreferenceManager(ISeasonalAuditPreferenceStore store, IClock clock, Registry flags)
    {
        return new SeasonalAuditPreferenceManager(
            store,
            SeasonalAuditPreferenceRules.DefaultSeasonalAuditPreferenceValidator(),
            clock,
            new List<string>(),
            SeasonalAuditPreferenceEngine.DefaultSeasonalAuditPreferenceEngine(flags));
    }
}
