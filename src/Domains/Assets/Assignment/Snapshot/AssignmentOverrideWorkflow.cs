namespace Acme.Workforce.Assets.Assignment.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves assignment overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssignmentOverrideWorkflow(IAssignmentOverrideStore Store, AssignmentOverrideValidator Validator, IClock Clock, List<string> Journal, AssignmentOverrideEngine Engine)
{
    public static AssignmentOverrideWorkflow NewAssignmentOverrideWorkflow(IAssignmentOverrideStore store, IClock clock)
    {
        return new AssignmentOverrideWorkflow(
            store,
            AssignmentOverrideRules.DefaultAssignmentOverrideValidator(),
            clock,
            new List<string>(),
            AssignmentOverrideEngine.DefaultAssignmentOverrideEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssignmentOverrideStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssignmentOverrideStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment override {code} not found");
        }
        var result = Engine.Run(item);
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
