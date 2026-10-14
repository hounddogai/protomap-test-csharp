namespace Acme.Workforce.Absence.ReturnPlan.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves return plan templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReturnPlanTemplateCoordinator(IReturnPlanTemplateStore Store, ReturnPlanTemplateValidator Validator, IClock Clock, List<string> Journal, ReturnPlanTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReturnPlanTemplateStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReturnPlanTemplateStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan template {code} not found");
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

/// <summary>Functions of return plan template workflow.</summary>
public static class ReturnPlanTemplateWorkflow
{
    public static ReturnPlanTemplateCoordinator NewReturnPlanTemplateCoordinator(IReturnPlanTemplateStore store, IClock clock)
    {
        return new ReturnPlanTemplateCoordinator(
            store,
            ReturnPlanTemplateRules.DefaultReturnPlanTemplateValidator(),
            clock,
            new List<string>(),
            ReturnPlanTemplateEngine.DefaultReturnPlanTemplateEngine());
    }
}
