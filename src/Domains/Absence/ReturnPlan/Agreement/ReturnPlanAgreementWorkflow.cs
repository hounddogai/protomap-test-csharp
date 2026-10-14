namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves return plan agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReturnPlanAgreementCoordinator(IReturnPlanAgreementStore Store, ReturnPlanAgreementValidator Validator, IClock Clock, List<string> Journal, ReturnPlanAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReturnPlanAgreementStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReturnPlanAgreementStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan agreement {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of return plan agreement workflow.</summary>
public static class ReturnPlanAgreementWorkflow
{
    public static ReturnPlanAgreementCoordinator NewReturnPlanAgreementCoordinator(IReturnPlanAgreementStore store, IClock clock)
    {
        return new ReturnPlanAgreementCoordinator(
            store,
            ReturnPlanAgreementRules.DefaultReturnPlanAgreementValidator(),
            clock,
            new List<string>(),
            ReturnPlanAgreementEngine.DefaultReturnPlanAgreementEngine());
    }
}
