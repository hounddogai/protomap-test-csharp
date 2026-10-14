namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves visitor plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorPlanWorkflow(IVisitorPlanStore Store, VisitorPlanValidator Validator, IClock Clock, List<string> Journal, VisitorPlanEngine Engine)
{
    public static VisitorPlanWorkflow NewVisitorPlanWorkflow(IVisitorPlanStore store, IClock clock)
    {
        return new VisitorPlanWorkflow(
            store,
            VisitorPlanRules.DefaultVisitorPlanValidator(),
            clock,
            new List<string>(),
            VisitorPlanEngine.DefaultVisitorPlanEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorPlanStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorPlanStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor plan {code} not found");
        }
        var result = Engine.Run(item);
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
