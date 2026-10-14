namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves mobility windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record MobilityWindowManager(IMobilityWindowStore Store, MobilityWindowValidator Validator, IClock Clock, List<string> Journal, MobilityWindowEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MobilityWindowStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MobilityWindowStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility window {code} not found");
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

/// <summary>Functions of mobility window workflow.</summary>
public static class MobilityWindowWorkflow
{
    public static MobilityWindowManager NewMobilityWindowManager(IMobilityWindowStore store, IClock clock)
    {
        return new MobilityWindowManager(
            store,
            MobilityWindowRules.DefaultMobilityWindowValidator(),
            clock,
            new List<string>(),
            MobilityWindowEngine.DefaultMobilityWindowEngine());
    }
}
