namespace Acme.Workforce.Assets.Laptop.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves laptop manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record LaptopManifestDesk(ILaptopManifestStore Store, LaptopManifestValidator Validator, IClock Clock, List<string> Journal, LaptopManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LaptopManifestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LaptopManifestStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop manifest {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of laptop manifest workflow.</summary>
public static class LaptopManifestWorkflow
{
    public static LaptopManifestDesk NewLaptopManifestDesk(ILaptopManifestStore store, IClock clock, Registry flags)
    {
        return new LaptopManifestDesk(
            store,
            LaptopManifestRules.DefaultLaptopManifestValidator(),
            clock,
            new List<string>(),
            LaptopManifestEngine.DefaultLaptopManifestEngine(flags));
    }
}
