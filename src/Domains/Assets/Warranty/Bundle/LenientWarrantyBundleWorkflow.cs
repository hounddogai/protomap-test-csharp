namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient warranty bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientWarrantyBundleManager(ILenientWarrantyBundleStore Store, LenientWarrantyBundleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient warranty bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientWarrantyBundleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient warranty bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientWarrantyBundleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of lenient warranty bundle workflow.</summary>
public static class LenientWarrantyBundleWorkflow
{
    public static LenientWarrantyBundleManager NewLenientWarrantyBundleManager(ILenientWarrantyBundleStore store, IClock clock)
    {
        return new LenientWarrantyBundleManager(
            store,
            LenientWarrantyBundleRules.DefaultLenientWarrantyBundleValidator(),
            clock,
            new List<string>());
    }
}
