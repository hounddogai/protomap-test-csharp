namespace Acme.Workforce.Compliance.Certification.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive certification plans through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveCertificationPlanService(IAdaptiveCertificationPlanStore Store, AdaptiveCertificationPlanValidator Validator, IClock Clock, List<string> Journal, AdaptiveCertificationPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive certification plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveCertificationPlanStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive certification plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveCertificationPlanStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive certification plan {code} not found");
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

/// <summary>Functions of adaptive certification plan workflow.</summary>
public static class AdaptiveCertificationPlanWorkflow
{
    public static AdaptiveCertificationPlanService NewAdaptiveCertificationPlanService(IAdaptiveCertificationPlanStore store, IClock clock, Registry flags)
    {
        return new AdaptiveCertificationPlanService(
            store,
            AdaptiveCertificationPlanRules.DefaultAdaptiveCertificationPlanValidator(),
            clock,
            new List<string>(),
            AdaptiveCertificationPlanEngine.DefaultAdaptiveCertificationPlanEngine(flags));
    }
}
