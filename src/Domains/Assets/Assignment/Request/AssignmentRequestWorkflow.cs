namespace Acme.Workforce.Assets.Assignment.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves assignment requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssignmentRequestWorkflow(IAssignmentRequestStore Store, AssignmentRequestValidator Validator, IClock Clock, List<string> Journal, AssignmentRequestEngine Engine)
{
    public static AssignmentRequestWorkflow NewAssignmentRequestWorkflow(IAssignmentRequestStore store, IClock clock)
    {
        return new AssignmentRequestWorkflow(
            store,
            AssignmentRequestRules.DefaultAssignmentRequestValidator(),
            clock,
            new List<string>(),
            AssignmentRequestEngine.DefaultAssignmentRequestEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssignmentRequestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssignmentRequestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment request {code} not found");
        }
        var result = Engine.Run(item);
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
