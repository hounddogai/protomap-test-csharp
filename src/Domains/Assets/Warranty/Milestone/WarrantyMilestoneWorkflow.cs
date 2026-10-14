namespace Acme.Workforce.Assets.Warranty.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves warranty milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyMilestoneDesk(IWarrantyMilestoneStore Store, WarrantyMilestoneValidator Validator, IClock Clock, List<string> Journal, WarrantyMilestoneEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyMilestoneStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyMilestoneStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty milestone {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of warranty milestone workflow.</summary>
public static class WarrantyMilestoneWorkflow
{
    public static WarrantyMilestoneDesk NewWarrantyMilestoneDesk(IWarrantyMilestoneStore store, IClock clock, Registry flags)
    {
        return new WarrantyMilestoneDesk(
            store,
            WarrantyMilestoneRules.DefaultWarrantyMilestoneValidator(),
            clock,
            new List<string>(),
            WarrantyMilestoneEngine.DefaultWarrantyMilestoneEngine(flags));
    }
}
