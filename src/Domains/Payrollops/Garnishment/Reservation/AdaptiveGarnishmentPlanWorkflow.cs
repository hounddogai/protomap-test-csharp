namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive garnishment plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveGarnishmentPlanService(IAdaptiveGarnishmentPlanStore Store, AdaptiveGarnishmentPlanValidator Validator, IClock Clock, List<string> Journal, AdaptiveGarnishmentPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive garnishment plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveGarnishmentPlanStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive garnishment plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveGarnishmentPlanStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive garnishment plan {code} not found");
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

/// <summary>Functions of adaptive garnishment plan workflow.</summary>
public static class AdaptiveGarnishmentPlanWorkflow
{
    public static AdaptiveGarnishmentPlanService NewAdaptiveGarnishmentPlanService(IAdaptiveGarnishmentPlanStore store, IClock clock)
    {
        return new AdaptiveGarnishmentPlanService(
            store,
            AdaptiveGarnishmentPlanRules.DefaultAdaptiveGarnishmentPlanValidator(),
            clock,
            new List<string>(),
            AdaptiveGarnishmentPlanEngine.DefaultAdaptiveGarnishmentPlanEngine());
    }
}
