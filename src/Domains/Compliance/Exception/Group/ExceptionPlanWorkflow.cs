namespace Acme.Workforce.Compliance.Exception.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves exception plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionPlanService(IExceptionPlanStore Store, ExceptionPlanValidator Validator, IClock Clock, List<string> Journal, ExceptionPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionPlanStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionPlanStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception plan {code} not found");
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

/// <summary>Functions of exception plan workflow.</summary>
public static class ExceptionPlanWorkflow
{
    public static ExceptionPlanService NewExceptionPlanService(IExceptionPlanStore store, IClock clock)
    {
        return new ExceptionPlanService(
            store,
            ExceptionPlanRules.DefaultExceptionPlanValidator(),
            clock,
            new List<string>(),
            ExceptionPlanEngine.DefaultExceptionPlanEngine());
    }
}
