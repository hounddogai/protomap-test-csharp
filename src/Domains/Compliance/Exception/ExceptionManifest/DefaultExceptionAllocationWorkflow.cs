namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default exception allocations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultExceptionAllocationWorkflow(IDefaultExceptionAllocationStore Store, DefaultExceptionAllocationValidator Validator, IClock Clock, List<string> Journal, DefaultExceptionAllocationEngine Engine)
{
    public static DefaultExceptionAllocationWorkflow NewDefaultExceptionAllocationWorkflow(IDefaultExceptionAllocationStore store, IClock clock)
    {
        return new DefaultExceptionAllocationWorkflow(
            store,
            DefaultExceptionAllocationRules.DefaultDefaultExceptionAllocationValidator(),
            clock,
            new List<string>(),
            DefaultExceptionAllocationEngine.DefaultDefaultExceptionAllocationEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default exception allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultExceptionAllocationStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default exception allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultExceptionAllocationStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default exception allocation {code} not found");
        }
        var result = Engine.Compute(item);
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
