namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient retro pay preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientRetroPayPreferenceDesk(ILenientRetroPayPreferenceStore Store, LenientRetroPayPreferenceValidator Validator, IClock Clock, List<string> Journal, LenientRetroPayPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient retro pay preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientRetroPayPreferenceStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient retro pay preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientRetroPayPreferenceStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient retro pay preference {code} not found");
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

/// <summary>Functions of lenient retro pay preference workflow.</summary>
public static class LenientRetroPayPreferenceWorkflow
{
    public static LenientRetroPayPreferenceDesk NewLenientRetroPayPreferenceDesk(ILenientRetroPayPreferenceStore store, IClock clock, Registry flags)
    {
        return new LenientRetroPayPreferenceDesk(
            store,
            LenientRetroPayPreferenceRules.DefaultLenientRetroPayPreferenceValidator(),
            clock,
            new List<string>(),
            LenientRetroPayPreferenceEngine.DefaultLenientRetroPayPreferenceEngine(flags));
    }
}
