namespace Acme.Workforce.Payrollops.Garnishment.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered garnishment quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredGarnishmentQuotaDesk(ITieredGarnishmentQuotaStore Store, TieredGarnishmentQuotaValidator Validator, IClock Clock, List<string> Journal, TieredGarnishmentQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered garnishment quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredGarnishmentQuotaStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered garnishment quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredGarnishmentQuotaStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered garnishment quota {code} not found");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of tiered garnishment quota workflow.</summary>
public static class TieredGarnishmentQuotaWorkflow
{
    public static TieredGarnishmentQuotaDesk NewTieredGarnishmentQuotaDesk(ITieredGarnishmentQuotaStore store, IClock clock, Registry flags)
    {
        return new TieredGarnishmentQuotaDesk(
            store,
            TieredGarnishmentQuotaRules.DefaultTieredGarnishmentQuotaValidator(),
            clock,
            new List<string>(),
            TieredGarnishmentQuotaEngine.DefaultTieredGarnishmentQuotaEngine(flags));
    }
}
