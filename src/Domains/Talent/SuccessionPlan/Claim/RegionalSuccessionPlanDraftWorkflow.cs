namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional succession plan drafts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalSuccessionPlanDraftCoordinator(IRegionalSuccessionPlanDraftStore Store, RegionalSuccessionPlanDraftValidator Validator, IClock Clock, List<string> Journal, RegionalSuccessionPlanDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional succession plan draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalSuccessionPlanDraftStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional succession plan draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalSuccessionPlanDraftStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional succession plan draft {code} not found");
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

/// <summary>Functions of regional succession plan draft workflow.</summary>
public static class RegionalSuccessionPlanDraftWorkflow
{
    public static RegionalSuccessionPlanDraftCoordinator NewRegionalSuccessionPlanDraftCoordinator(IRegionalSuccessionPlanDraftStore store, IClock clock, Registry flags)
    {
        return new RegionalSuccessionPlanDraftCoordinator(
            store,
            RegionalSuccessionPlanDraftRules.DefaultRegionalSuccessionPlanDraftValidator(),
            clock,
            new List<string>(),
            RegionalSuccessionPlanDraftEngine.DefaultRegionalSuccessionPlanDraftEngine(flags));
    }
}
