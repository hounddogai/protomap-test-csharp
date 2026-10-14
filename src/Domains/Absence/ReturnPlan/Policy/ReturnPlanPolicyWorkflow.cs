namespace Acme.Workforce.Absence.ReturnPlan.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves return plan policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReturnPlanPolicyCoordinator(IReturnPlanPolicyStore Store, ReturnPlanPolicyValidator Validator, IClock Clock, List<string> Journal, ReturnPlanPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReturnPlanPolicyStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReturnPlanPolicyStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan policy {code} not found");
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

/// <summary>Functions of return plan policy workflow.</summary>
public static class ReturnPlanPolicyWorkflow
{
    public static ReturnPlanPolicyCoordinator NewReturnPlanPolicyCoordinator(IReturnPlanPolicyStore store, IClock clock, Registry flags)
    {
        return new ReturnPlanPolicyCoordinator(
            store,
            ReturnPlanPolicyRules.DefaultReturnPlanPolicyValidator(),
            clock,
            new List<string>(),
            ReturnPlanPolicyEngine.DefaultReturnPlanPolicyEngine(flags));
    }
}
