namespace Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback succession plan manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackSuccessionPlanManifestDesk(IFallbackSuccessionPlanManifestStore Store, FallbackSuccessionPlanManifestValidator Validator, IClock Clock, List<string> Journal, FallbackSuccessionPlanManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback succession plan manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackSuccessionPlanManifestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback succession plan manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackSuccessionPlanManifestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback succession plan manifest {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of fallback succession plan manifest workflow.</summary>
public static class FallbackSuccessionPlanManifestWorkflow
{
    public static FallbackSuccessionPlanManifestDesk NewFallbackSuccessionPlanManifestDesk(IFallbackSuccessionPlanManifestStore store, IClock clock, Registry flags)
    {
        return new FallbackSuccessionPlanManifestDesk(
            store,
            FallbackSuccessionPlanManifestRules.DefaultFallbackSuccessionPlanManifestValidator(),
            clock,
            new List<string>(),
            FallbackSuccessionPlanManifestEngine.DefaultFallbackSuccessionPlanManifestEngine(flags));
    }
}
