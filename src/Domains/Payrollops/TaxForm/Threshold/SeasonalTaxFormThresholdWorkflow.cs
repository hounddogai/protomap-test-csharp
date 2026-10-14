namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal tax form thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalTaxFormThresholdManager(ISeasonalTaxFormThresholdStore Store, SeasonalTaxFormThresholdValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal tax form threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalTaxFormThresholdStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal tax form threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalTaxFormThresholdStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of seasonal tax form threshold workflow.</summary>
public static class SeasonalTaxFormThresholdWorkflow
{
    public static SeasonalTaxFormThresholdManager NewSeasonalTaxFormThresholdManager(ISeasonalTaxFormThresholdStore store, IClock clock)
    {
        return new SeasonalTaxFormThresholdManager(
            store,
            SeasonalTaxFormThresholdRules.DefaultSeasonalTaxFormThresholdValidator(),
            clock,
            new List<string>());
    }
}
