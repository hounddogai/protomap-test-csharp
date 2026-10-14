namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default succession plan variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultSuccessionPlanVarianceWorkflow(IDefaultSuccessionPlanVarianceStore Store, DefaultSuccessionPlanVarianceValidator Validator, IClock Clock, List<string> Journal, DefaultSuccessionPlanVarianceEngine Engine)
{
    public static DefaultSuccessionPlanVarianceWorkflow NewDefaultSuccessionPlanVarianceWorkflow(IDefaultSuccessionPlanVarianceStore store, IClock clock, Registry flags)
    {
        return new DefaultSuccessionPlanVarianceWorkflow(
            store,
            DefaultSuccessionPlanVarianceRules.DefaultDefaultSuccessionPlanVarianceValidator(),
            clock,
            new List<string>(),
            DefaultSuccessionPlanVarianceEngine.DefaultDefaultSuccessionPlanVarianceEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default succession plan variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultSuccessionPlanVarianceStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default succession plan variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultSuccessionPlanVarianceStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default succession plan variance {code} not found");
        }
        var result = Engine.Evaluate(item);
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
