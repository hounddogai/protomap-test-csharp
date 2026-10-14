namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyPolicyCoordinator(IWarrantyPolicyStore Store, WarrantyPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyPolicyStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyPolicyStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of warranty policy workflow.</summary>
public static class WarrantyPolicyWorkflow
{
    public static WarrantyPolicyCoordinator NewWarrantyPolicyCoordinator(IWarrantyPolicyStore store, IClock clock)
    {
        return new WarrantyPolicyCoordinator(
            store,
            WarrantyPolicyRules.DefaultWarrantyPolicyValidator(),
            clock,
            new List<string>());
    }
}
