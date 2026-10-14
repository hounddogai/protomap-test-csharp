namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual laptop bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualLaptopBundleDesk(IManualLaptopBundleStore Store, ManualLaptopBundleValidator Validator, IClock Clock, List<string> Journal, ManualLaptopBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual laptop bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualLaptopBundleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual laptop bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualLaptopBundleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual laptop bundle {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of manual laptop bundle workflow.</summary>
public static class ManualLaptopBundleWorkflow
{
    public static ManualLaptopBundleDesk NewManualLaptopBundleDesk(IManualLaptopBundleStore store, IClock clock, Registry flags)
    {
        return new ManualLaptopBundleDesk(
            store,
            ManualLaptopBundleRules.DefaultManualLaptopBundleValidator(),
            clock,
            new List<string>(),
            ManualLaptopBundleEngine.DefaultManualLaptopBundleEngine(flags));
    }
}
