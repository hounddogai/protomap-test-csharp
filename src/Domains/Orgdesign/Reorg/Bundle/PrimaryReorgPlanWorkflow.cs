namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary reorg plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryReorgPlanManager(IPrimaryReorgPlanStore Store, PrimaryReorgPlanValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary reorg plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryReorgPlanStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary reorg plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryReorgPlanStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of primary reorg plan workflow.</summary>
public static class PrimaryReorgPlanWorkflow
{
    public static PrimaryReorgPlanManager NewPrimaryReorgPlanManager(IPrimaryReorgPlanStore store, IClock clock)
    {
        return new PrimaryReorgPlanManager(
            store,
            PrimaryReorgPlanRules.DefaultPrimaryReorgPlanValidator(),
            clock,
            new List<string>());
    }
}
