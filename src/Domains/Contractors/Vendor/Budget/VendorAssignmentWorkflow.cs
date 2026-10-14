namespace Acme.Workforce.Contractors.Vendor.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves vendor assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorAssignmentManager(IVendorAssignmentStore Store, VendorAssignmentValidator Validator, IClock Clock, List<string> Journal, VendorAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorAssignmentStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorAssignmentStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor assignment {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of vendor assignment workflow.</summary>
public static class VendorAssignmentWorkflow
{
    public static VendorAssignmentManager NewVendorAssignmentManager(IVendorAssignmentStore store, IClock clock, Registry flags)
    {
        return new VendorAssignmentManager(
            store,
            VendorAssignmentRules.DefaultVendorAssignmentValidator(),
            clock,
            new List<string>(),
            VendorAssignmentEngine.DefaultVendorAssignmentEngine(flags));
    }
}
