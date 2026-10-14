namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient scenario cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientScenarioCycleDesk(ILenientScenarioCycleStore Store, LenientScenarioCycleValidator Validator, IClock Clock, List<string> Journal, LenientScenarioCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient scenario cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientScenarioCycleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient scenario cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientScenarioCycleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient scenario cycle {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of lenient scenario cycle workflow.</summary>
public static class LenientScenarioCycleWorkflow
{
    public static LenientScenarioCycleDesk NewLenientScenarioCycleDesk(ILenientScenarioCycleStore store, IClock clock, Registry flags)
    {
        return new LenientScenarioCycleDesk(
            store,
            LenientScenarioCycleRules.DefaultLenientScenarioCycleValidator(),
            clock,
            new List<string>(),
            LenientScenarioCycleEngine.DefaultLenientScenarioCycleEngine(flags));
    }
}
