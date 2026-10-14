namespace Acme.Workforce.Assets.Warranty.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyAssignmentCoordinator(IWarrantyAssignmentStore Store, WarrantyAssignmentValidator Validator, IClock Clock, List<string> Journal, WarrantyAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyAssignmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyAssignmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty assignment {code} not found");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of warranty assignment workflow.</summary>
public static class WarrantyAssignmentWorkflow
{
    public static WarrantyAssignmentCoordinator NewWarrantyAssignmentCoordinator(IWarrantyAssignmentStore store, IClock clock)
    {
        return new WarrantyAssignmentCoordinator(
            store,
            WarrantyAssignmentRules.DefaultWarrantyAssignmentValidator(),
            clock,
            new List<string>(),
            WarrantyAssignmentEngine.DefaultWarrantyAssignmentEngine());
    }
}
