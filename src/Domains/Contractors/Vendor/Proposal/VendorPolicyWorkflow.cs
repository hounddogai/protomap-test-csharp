namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves vendor policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorPolicyManager(IVendorPolicyStore Store, VendorPolicyValidator Validator, IClock Clock, List<string> Journal, VendorPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorPolicyStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorPolicyStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor policy {code} not found");
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

/// <summary>Functions of vendor policy workflow.</summary>
public static class VendorPolicyWorkflow
{
    public static VendorPolicyManager NewVendorPolicyManager(IVendorPolicyStore store, IClock clock, Registry flags)
    {
        return new VendorPolicyManager(
            store,
            VendorPolicyRules.DefaultVendorPolicyValidator(),
            clock,
            new List<string>(),
            VendorPolicyEngine.DefaultVendorPolicyEngine(flags));
    }
}
