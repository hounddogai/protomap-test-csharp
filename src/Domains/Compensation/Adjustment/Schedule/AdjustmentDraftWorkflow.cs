namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adjustment drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentDraftWorkflow(IAdjustmentDraftStore Store, AdjustmentDraftValidator Validator, IClock Clock, List<string> Journal)
{
    public static AdjustmentDraftWorkflow NewAdjustmentDraftWorkflow(IAdjustmentDraftStore store, IClock clock)
    {
        return new AdjustmentDraftWorkflow(
            store,
            AdjustmentDraftRules.DefaultAdjustmentDraftValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentDraftStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentDraftStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
