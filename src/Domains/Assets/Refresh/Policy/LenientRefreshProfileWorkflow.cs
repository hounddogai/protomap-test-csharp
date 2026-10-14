namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient refresh profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientRefreshProfileDesk(ILenientRefreshProfileStore Store, LenientRefreshProfileValidator Validator, IClock Clock, List<string> Journal, LenientRefreshProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient refresh profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientRefreshProfileStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient refresh profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientRefreshProfileStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient refresh profile {code} not found");
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

/// <summary>Functions of lenient refresh profile workflow.</summary>
public static class LenientRefreshProfileWorkflow
{
    public static LenientRefreshProfileDesk NewLenientRefreshProfileDesk(ILenientRefreshProfileStore store, IClock clock)
    {
        return new LenientRefreshProfileDesk(
            store,
            LenientRefreshProfileRules.DefaultLenientRefreshProfileValidator(),
            clock,
            new List<string>(),
            LenientRefreshProfileEngine.DefaultLenientRefreshProfileEngine());
    }
}
