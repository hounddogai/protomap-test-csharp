namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves exception allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionAllocationDesk(IExceptionAllocationStore Store, ExceptionAllocationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionAllocationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionAllocationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of exception allocation workflow.</summary>
public static class ExceptionAllocationWorkflow
{
    public static ExceptionAllocationDesk NewExceptionAllocationDesk(IExceptionAllocationStore store, IClock clock)
    {
        return new ExceptionAllocationDesk(
            store,
            ExceptionAllocationRules.DefaultExceptionAllocationValidator(),
            clock,
            new List<string>());
    }
}
