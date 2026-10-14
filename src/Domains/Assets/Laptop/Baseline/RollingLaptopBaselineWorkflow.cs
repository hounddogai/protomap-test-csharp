namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling laptop baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingLaptopBaselineManager(IRollingLaptopBaselineStore Store, RollingLaptopBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling laptop baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingLaptopBaselineStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling laptop baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingLaptopBaselineStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of rolling laptop baseline workflow.</summary>
public static class RollingLaptopBaselineWorkflow
{
    public static RollingLaptopBaselineManager NewRollingLaptopBaselineManager(IRollingLaptopBaselineStore store, IClock clock)
    {
        return new RollingLaptopBaselineManager(
            store,
            RollingLaptopBaselineRules.DefaultRollingLaptopBaselineValidator(),
            clock,
            new List<string>());
    }
}
