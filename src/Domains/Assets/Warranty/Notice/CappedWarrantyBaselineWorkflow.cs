namespace Acme.Workforce.Assets.Warranty.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped warranty baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedWarrantyBaselineManager(ICappedWarrantyBaselineStore Store, CappedWarrantyBaselineValidator Validator, IClock Clock, List<string> Journal, CappedWarrantyBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped warranty baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedWarrantyBaselineStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped warranty baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedWarrantyBaselineStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped warranty baseline {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of capped warranty baseline workflow.</summary>
public static class CappedWarrantyBaselineWorkflow
{
    public static CappedWarrantyBaselineManager NewCappedWarrantyBaselineManager(ICappedWarrantyBaselineStore store, IClock clock)
    {
        return new CappedWarrantyBaselineManager(
            store,
            CappedWarrantyBaselineRules.DefaultCappedWarrantyBaselineValidator(),
            clock,
            new List<string>(),
            CappedWarrantyBaselineEngine.DefaultCappedWarrantyBaselineEngine());
    }
}
