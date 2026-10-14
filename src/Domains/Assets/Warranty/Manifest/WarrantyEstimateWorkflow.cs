namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyEstimateDesk(IWarrantyEstimateStore Store, WarrantyEstimateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyEstimateStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyEstimateStatus.Retired);
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

/// <summary>Functions of warranty estimate workflow.</summary>
public static class WarrantyEstimateWorkflow
{
    public static WarrantyEstimateDesk NewWarrantyEstimateDesk(IWarrantyEstimateStore store, IClock clock)
    {
        return new WarrantyEstimateDesk(
            store,
            WarrantyEstimateRules.DefaultWarrantyEstimateValidator(),
            clock,
            new List<string>());
    }
}
