namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves laptop baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record LaptopBaselineCoordinator(ILaptopBaselineStore Store, LaptopBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LaptopBaselineStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LaptopBaselineStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of laptop baseline workflow.</summary>
public static class LaptopBaselineWorkflow
{
    public static LaptopBaselineCoordinator NewLaptopBaselineCoordinator(ILaptopBaselineStore store, IClock clock)
    {
        return new LaptopBaselineCoordinator(
            store,
            LaptopBaselineRules.DefaultLaptopBaselineValidator(),
            clock,
            new List<string>());
    }
}
