namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default retro pay allocations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultRetroPayAllocationDesk(IDefaultRetroPayAllocationStore Store, DefaultRetroPayAllocationValidator Validator, IClock Clock, List<string> Journal, DefaultRetroPayAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default retro pay allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultRetroPayAllocationStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default retro pay allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultRetroPayAllocationStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default retro pay allocation {code} not found");
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

/// <summary>Functions of default retro pay allocation workflow.</summary>
public static class DefaultRetroPayAllocationWorkflow
{
    public static DefaultRetroPayAllocationDesk NewDefaultRetroPayAllocationDesk(IDefaultRetroPayAllocationStore store, IClock clock, Registry flags)
    {
        return new DefaultRetroPayAllocationDesk(
            store,
            DefaultRetroPayAllocationRules.DefaultDefaultRetroPayAllocationValidator(),
            clock,
            new List<string>(),
            DefaultRetroPayAllocationEngine.DefaultDefaultRetroPayAllocationEngine(flags));
    }
}
