namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict audit metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictAuditMetricManager(IStrictAuditMetricStore Store, StrictAuditMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict audit metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictAuditMetricStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict audit metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictAuditMetricStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of strict audit metric workflow.</summary>
public static class StrictAuditMetricWorkflow
{
    public static StrictAuditMetricManager NewStrictAuditMetricManager(IStrictAuditMetricStore store, IClock clock)
    {
        return new StrictAuditMetricManager(
            store,
            StrictAuditMetricRules.DefaultStrictAuditMetricValidator(),
            clock,
            new List<string>());
    }
}
