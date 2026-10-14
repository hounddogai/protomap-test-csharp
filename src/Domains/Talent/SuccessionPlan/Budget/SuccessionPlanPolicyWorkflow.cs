namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves succession plan policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record SuccessionPlanPolicyService(ISuccessionPlanPolicyStore Store, SuccessionPlanPolicyValidator Validator, IClock Clock, List<string> Journal, SuccessionPlanPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SuccessionPlanPolicyStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SuccessionPlanPolicyStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan policy {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of succession plan policy workflow.</summary>
public static class SuccessionPlanPolicyWorkflow
{
    public static SuccessionPlanPolicyService NewSuccessionPlanPolicyService(ISuccessionPlanPolicyStore store, IClock clock)
    {
        return new SuccessionPlanPolicyService(
            store,
            SuccessionPlanPolicyRules.DefaultSuccessionPlanPolicyValidator(),
            clock,
            new List<string>(),
            SuccessionPlanPolicyEngine.DefaultSuccessionPlanPolicyEngine());
    }
}
