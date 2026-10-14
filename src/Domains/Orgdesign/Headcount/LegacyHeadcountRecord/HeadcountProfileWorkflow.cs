namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves headcount profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountProfileWorkflow(IHeadcountProfileStore Store, HeadcountProfileValidator Validator, IClock Clock, List<string> Journal, HeadcountProfileEngine Engine)
{
    public static HeadcountProfileWorkflow NewHeadcountProfileWorkflow(IHeadcountProfileStore store, IClock clock)
    {
        return new HeadcountProfileWorkflow(
            store,
            HeadcountProfileRules.DefaultHeadcountProfileValidator(),
            clock,
            new List<string>(),
            HeadcountProfileEngine.DefaultHeadcountProfileEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountProfileStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountProfileStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount profile {code} not found");
        }
        var result = Engine.Compute(item);
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
