namespace Acme.Workforce.Workplace.Visitor.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves visitor targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorTargetWorkflow(IVisitorTargetStore Store, VisitorTargetValidator Validator, IClock Clock, List<string> Journal, VisitorTargetEngine Engine)
{
    public static VisitorTargetWorkflow NewVisitorTargetWorkflow(IVisitorTargetStore store, IClock clock, Registry flags)
    {
        return new VisitorTargetWorkflow(
            store,
            VisitorTargetRules.DefaultVisitorTargetValidator(),
            clock,
            new List<string>(),
            VisitorTargetEngine.DefaultVisitorTargetEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorTargetStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorTargetStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor target {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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
