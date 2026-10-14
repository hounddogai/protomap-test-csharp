namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling succession plan quotas through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingSuccessionPlanQuotaWorkflow(IRollingSuccessionPlanQuotaStore Store, RollingSuccessionPlanQuotaValidator Validator, IClock Clock, List<string> Journal, RollingSuccessionPlanQuotaEngine Engine)
{
    public static RollingSuccessionPlanQuotaWorkflow NewRollingSuccessionPlanQuotaWorkflow(IRollingSuccessionPlanQuotaStore store, IClock clock, Registry flags)
    {
        return new RollingSuccessionPlanQuotaWorkflow(
            store,
            RollingSuccessionPlanQuotaRules.DefaultRollingSuccessionPlanQuotaValidator(),
            clock,
            new List<string>(),
            RollingSuccessionPlanQuotaEngine.DefaultRollingSuccessionPlanQuotaEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling succession plan quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingSuccessionPlanQuotaStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling succession plan quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingSuccessionPlanQuotaStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling succession plan quota {code} not found");
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
