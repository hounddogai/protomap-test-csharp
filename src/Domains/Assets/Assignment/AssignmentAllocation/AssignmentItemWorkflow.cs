namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves assignment items through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssignmentItemDesk(IAssignmentItemStore Store, AssignmentItemValidator Validator, IClock Clock, List<string> Journal, AssignmentItemEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment item {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssignmentItemStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment item {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssignmentItemStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment item {code} not found");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of assignment item workflow.</summary>
public static class AssignmentItemWorkflow
{
    public static AssignmentItemDesk NewAssignmentItemDesk(IAssignmentItemStore store, IClock clock)
    {
        return new AssignmentItemDesk(
            store,
            AssignmentItemRules.DefaultAssignmentItemValidator(),
            clock,
            new List<string>(),
            AssignmentItemEngine.DefaultAssignmentItemEngine());
    }
}
