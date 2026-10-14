namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted warranty drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedWarrantyDraftService(IWeightedWarrantyDraftStore Store, WeightedWarrantyDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted warranty draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedWarrantyDraftStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted warranty draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedWarrantyDraftStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of weighted warranty draft workflow.</summary>
public static class WeightedWarrantyDraftWorkflow
{
    public static WeightedWarrantyDraftService NewWeightedWarrantyDraftService(IWeightedWarrantyDraftStore store, IClock clock)
    {
        return new WeightedWarrantyDraftService(
            store,
            WeightedWarrantyDraftRules.DefaultWeightedWarrantyDraftValidator(),
            clock,
            new List<string>());
    }
}
