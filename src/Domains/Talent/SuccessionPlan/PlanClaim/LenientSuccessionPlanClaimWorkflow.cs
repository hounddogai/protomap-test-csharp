namespace Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient succession plan claims through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientSuccessionPlanClaimManager(ILenientSuccessionPlanClaimStore Store, LenientSuccessionPlanClaimValidator Validator, IClock Clock, List<string> Journal, LenientSuccessionPlanClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient succession plan claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientSuccessionPlanClaimStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient succession plan claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientSuccessionPlanClaimStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient succession plan claim {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of lenient succession plan claim workflow.</summary>
public static class LenientSuccessionPlanClaimWorkflow
{
    public static LenientSuccessionPlanClaimManager NewLenientSuccessionPlanClaimManager(ILenientSuccessionPlanClaimStore store, IClock clock, Registry flags)
    {
        return new LenientSuccessionPlanClaimManager(
            store,
            LenientSuccessionPlanClaimRules.DefaultLenientSuccessionPlanClaimValidator(),
            clock,
            new List<string>(),
            LenientSuccessionPlanClaimEngine.DefaultLenientSuccessionPlanClaimEngine(flags));
    }
}
