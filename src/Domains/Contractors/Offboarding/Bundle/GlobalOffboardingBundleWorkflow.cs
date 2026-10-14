namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global offboarding bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalOffboardingBundleDesk(IGlobalOffboardingBundleStore Store, GlobalOffboardingBundleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global offboarding bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalOffboardingBundleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global offboarding bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalOffboardingBundleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of global offboarding bundle workflow.</summary>
public static class GlobalOffboardingBundleWorkflow
{
    public static GlobalOffboardingBundleDesk NewGlobalOffboardingBundleDesk(IGlobalOffboardingBundleStore store, IClock clock)
    {
        return new GlobalOffboardingBundleDesk(
            store,
            GlobalOffboardingBundleRules.DefaultGlobalOffboardingBundleValidator(),
            clock,
            new List<string>());
    }
}
