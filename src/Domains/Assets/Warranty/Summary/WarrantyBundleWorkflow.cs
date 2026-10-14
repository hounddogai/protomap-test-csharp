namespace Acme.Workforce.Assets.Warranty.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves warranty bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyBundleWorkflow(IWarrantyBundleStore Store, WarrantyBundleValidator Validator, IClock Clock, List<string> Journal, WarrantyBundleEngine Engine)
{
    public static WarrantyBundleWorkflow NewWarrantyBundleWorkflow(IWarrantyBundleStore store, IClock clock, Registry flags)
    {
        return new WarrantyBundleWorkflow(
            store,
            WarrantyBundleRules.DefaultWarrantyBundleValidator(),
            clock,
            new List<string>(),
            WarrantyBundleEngine.DefaultWarrantyBundleEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyBundleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyBundleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty bundle {code} not found");
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
            if (item.ExpiresAt > cutoff)
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
