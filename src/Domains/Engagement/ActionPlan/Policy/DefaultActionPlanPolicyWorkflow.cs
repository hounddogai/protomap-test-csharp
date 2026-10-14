namespace Acme.Workforce.Engagement.ActionPlan.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default action plan policies through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultActionPlanPolicyManager(IDefaultActionPlanPolicyStore Store, DefaultActionPlanPolicyValidator Validator, IClock Clock, List<string> Journal, DefaultActionPlanPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default action plan policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultActionPlanPolicyStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default action plan policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultActionPlanPolicyStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default action plan policy {code} not found");
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

/// <summary>Functions of default action plan policy workflow.</summary>
public static class DefaultActionPlanPolicyWorkflow
{
    public static DefaultActionPlanPolicyManager NewDefaultActionPlanPolicyManager(IDefaultActionPlanPolicyStore store, IClock clock, Registry flags)
    {
        return new DefaultActionPlanPolicyManager(
            store,
            DefaultActionPlanPolicyRules.DefaultDefaultActionPlanPolicyValidator(),
            clock,
            new List<string>(),
            DefaultActionPlanPolicyEngine.DefaultDefaultActionPlanPolicyEngine(flags));
    }
}
