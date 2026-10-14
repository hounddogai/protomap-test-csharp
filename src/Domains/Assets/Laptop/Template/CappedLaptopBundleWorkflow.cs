namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped laptop bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedLaptopBundleDesk(ICappedLaptopBundleStore Store, CappedLaptopBundleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped laptop bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedLaptopBundleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped laptop bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedLaptopBundleStatus.Expired);
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

/// <summary>Functions of capped laptop bundle workflow.</summary>
public static class CappedLaptopBundleWorkflow
{
    public static CappedLaptopBundleDesk NewCappedLaptopBundleDesk(ICappedLaptopBundleStore store, IClock clock)
    {
        return new CappedLaptopBundleDesk(
            store,
            CappedLaptopBundleRules.DefaultCappedLaptopBundleValidator(),
            clock,
            new List<string>());
    }
}
