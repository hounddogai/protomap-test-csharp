namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyBaselineDesk(IWarrantyBaselineStore Store, WarrantyBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyBaselineStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyBaselineStatus.Failed);
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

/// <summary>Functions of warranty baseline workflow.</summary>
public static class WarrantyBaselineWorkflow
{
    public static WarrantyBaselineDesk NewWarrantyBaselineDesk(IWarrantyBaselineStore store, IClock clock)
    {
        return new WarrantyBaselineDesk(
            store,
            WarrantyBaselineRules.DefaultWarrantyBaselineValidator(),
            clock,
            new List<string>());
    }
}
