namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback garnishment plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackGarnishmentPlanWorkflow(IFallbackGarnishmentPlanStore Store, FallbackGarnishmentPlanValidator Validator, IClock Clock, List<string> Journal, FallbackGarnishmentPlanEngine Engine)
{
    public static FallbackGarnishmentPlanWorkflow NewFallbackGarnishmentPlanWorkflow(IFallbackGarnishmentPlanStore store, IClock clock)
    {
        return new FallbackGarnishmentPlanWorkflow(
            store,
            FallbackGarnishmentPlanRules.DefaultFallbackGarnishmentPlanValidator(),
            clock,
            new List<string>(),
            FallbackGarnishmentPlanEngine.DefaultFallbackGarnishmentPlanEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback garnishment plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackGarnishmentPlanStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback garnishment plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackGarnishmentPlanStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback garnishment plan {code} not found");
        }
        var result = Engine.Price(item);
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
