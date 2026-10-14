namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted license seat bundles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedLicenseSeatBundleService(IWeightedLicenseSeatBundleStore Store, WeightedLicenseSeatBundleValidator Validator, IClock Clock, List<string> Journal, WeightedLicenseSeatBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted license seat bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedLicenseSeatBundleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted license seat bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedLicenseSeatBundleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted license seat bundle {code} not found");
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

/// <summary>Functions of weighted license seat bundle workflow.</summary>
public static class WeightedLicenseSeatBundleWorkflow
{
    public static WeightedLicenseSeatBundleService NewWeightedLicenseSeatBundleService(IWeightedLicenseSeatBundleStore store, IClock clock)
    {
        return new WeightedLicenseSeatBundleService(
            store,
            WeightedLicenseSeatBundleRules.DefaultWeightedLicenseSeatBundleValidator(),
            clock,
            new List<string>(),
            WeightedLicenseSeatBundleEngine.DefaultWeightedLicenseSeatBundleEngine());
    }
}
