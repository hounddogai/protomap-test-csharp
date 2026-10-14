namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves vendor milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorMilestoneService(IVendorMilestoneStore Store, VendorMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorMilestoneStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorMilestoneStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of vendor milestone workflow.</summary>
public static class VendorMilestoneWorkflow
{
    public static VendorMilestoneService NewVendorMilestoneService(IVendorMilestoneStore store, IClock clock)
    {
        return new VendorMilestoneService(
            store,
            VendorMilestoneRules.DefaultVendorMilestoneValidator(),
            clock,
            new List<string>());
    }
}
