namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient merit cycle targets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientMeritCycleTargetWorkflow(ILenientMeritCycleTargetStore Store, LenientMeritCycleTargetValidator Validator, IClock Clock, List<string> Journal, LenientMeritCycleTargetEngine Engine)
{
    public static LenientMeritCycleTargetWorkflow NewLenientMeritCycleTargetWorkflow(ILenientMeritCycleTargetStore store, IClock clock, Registry flags)
    {
        return new LenientMeritCycleTargetWorkflow(
            store,
            LenientMeritCycleTargetRules.DefaultLenientMeritCycleTargetValidator(),
            clock,
            new List<string>(),
            LenientMeritCycleTargetEngine.DefaultLenientMeritCycleTargetEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient merit cycle target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientMeritCycleTargetStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient merit cycle target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientMeritCycleTargetStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient merit cycle target {code} not found");
        }
        var result = Engine.Evaluate(item);
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
