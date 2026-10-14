namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy headcount quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyHeadcountQuotaWorkflow(ILegacyHeadcountQuotaStore Store, LegacyHeadcountQuotaValidator Validator, IClock Clock, List<string> Journal, LegacyHeadcountQuotaEngine Engine)
{
    public static LegacyHeadcountQuotaWorkflow NewLegacyHeadcountQuotaWorkflow(ILegacyHeadcountQuotaStore store, IClock clock)
    {
        return new LegacyHeadcountQuotaWorkflow(
            store,
            LegacyHeadcountQuotaRules.DefaultLegacyHeadcountQuotaValidator(),
            clock,
            new List<string>(),
            LegacyHeadcountQuotaEngine.DefaultLegacyHeadcountQuotaEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy headcount quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyHeadcountQuotaStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy headcount quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyHeadcountQuotaStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy headcount quota {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
