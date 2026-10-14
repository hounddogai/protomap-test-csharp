namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves assignment entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssignmentEntryService(IAssignmentEntryStore Store, AssignmentEntryValidator Validator, IClock Clock, List<string> Journal, AssignmentEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssignmentEntryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssignmentEntryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment entry {code} not found");
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

/// <summary>Functions of assignment entry workflow.</summary>
public static class AssignmentEntryWorkflow
{
    public static AssignmentEntryService NewAssignmentEntryService(IAssignmentEntryStore store, IClock clock, Registry flags)
    {
        return new AssignmentEntryService(
            store,
            AssignmentEntryRules.DefaultAssignmentEntryValidator(),
            clock,
            new List<string>(),
            AssignmentEntryEngine.DefaultAssignmentEntryEngine(flags));
    }
}
