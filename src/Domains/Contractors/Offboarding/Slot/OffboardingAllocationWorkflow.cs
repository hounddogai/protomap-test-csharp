namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves offboarding allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingAllocationWorkflow(IOffboardingAllocationStore Store, OffboardingAllocationValidator Validator, IClock Clock, List<string> Journal)
{
    public static OffboardingAllocationWorkflow NewOffboardingAllocationWorkflow(IOffboardingAllocationStore store, IClock clock)
    {
        return new OffboardingAllocationWorkflow(
            store,
            OffboardingAllocationRules.DefaultOffboardingAllocationValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingAllocationStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingAllocationStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
