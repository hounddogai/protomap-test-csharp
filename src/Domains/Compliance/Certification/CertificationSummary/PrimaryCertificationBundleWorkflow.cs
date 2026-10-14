namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary certification bundles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryCertificationBundleCoordinator(IPrimaryCertificationBundleStore Store, PrimaryCertificationBundleValidator Validator, IClock Clock, List<string> Journal, PrimaryCertificationBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary certification bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryCertificationBundleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary certification bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryCertificationBundleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary certification bundle {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of primary certification bundle workflow.</summary>
public static class PrimaryCertificationBundleWorkflow
{
    public static PrimaryCertificationBundleCoordinator NewPrimaryCertificationBundleCoordinator(IPrimaryCertificationBundleStore store, IClock clock, Registry flags)
    {
        return new PrimaryCertificationBundleCoordinator(
            store,
            PrimaryCertificationBundleRules.DefaultPrimaryCertificationBundleValidator(),
            clock,
            new List<string>(),
            PrimaryCertificationBundleEngine.DefaultPrimaryCertificationBundleEngine(flags));
    }
}
