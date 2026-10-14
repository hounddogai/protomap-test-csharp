namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves garnishment cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record GarnishmentCycleDesk(IGarnishmentCycleStore Store, GarnishmentCycleValidator Validator, IClock Clock, List<string> Journal, GarnishmentCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GarnishmentCycleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GarnishmentCycleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment cycle {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of garnishment cycle workflow.</summary>
public static class GarnishmentCycleWorkflow
{
    public static GarnishmentCycleDesk NewGarnishmentCycleDesk(IGarnishmentCycleStore store, IClock clock, Registry flags)
    {
        return new GarnishmentCycleDesk(
            store,
            GarnishmentCycleRules.DefaultGarnishmentCycleValidator(),
            clock,
            new List<string>(),
            GarnishmentCycleEngine.DefaultGarnishmentCycleEngine(flags));
    }
}
