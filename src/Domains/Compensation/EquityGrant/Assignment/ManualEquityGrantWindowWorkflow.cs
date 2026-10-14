namespace Acme.Workforce.Compensation.EquityGrant.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual equity grant windows through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualEquityGrantWindowCoordinator(IManualEquityGrantWindowStore Store, ManualEquityGrantWindowValidator Validator, IClock Clock, List<string> Journal, ManualEquityGrantWindowEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual equity grant window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualEquityGrantWindowStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual equity grant window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualEquityGrantWindowStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual equity grant window {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of manual equity grant window workflow.</summary>
public static class ManualEquityGrantWindowWorkflow
{
    public static ManualEquityGrantWindowCoordinator NewManualEquityGrantWindowCoordinator(IManualEquityGrantWindowStore store, IClock clock, Registry flags)
    {
        return new ManualEquityGrantWindowCoordinator(
            store,
            ManualEquityGrantWindowRules.DefaultManualEquityGrantWindowValidator(),
            clock,
            new List<string>(),
            ManualEquityGrantWindowEngine.DefaultManualEquityGrantWindowEngine(flags));
    }
}
