namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual rate overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualRateOverrideService(IManualRateOverrideStore Store, ManualRateOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual rate override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualRateOverrideStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual rate override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualRateOverrideStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of manual rate override workflow.</summary>
public static class ManualRateOverrideWorkflow
{
    public static ManualRateOverrideService NewManualRateOverrideService(IManualRateOverrideStore store, IClock clock)
    {
        return new ManualRateOverrideService(
            store,
            ManualRateOverrideRules.DefaultManualRateOverrideValidator(),
            clock,
            new List<string>());
    }
}
