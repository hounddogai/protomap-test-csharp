namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback access badge overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackAccessBadgeOverrideWorkflow(IFallbackAccessBadgeOverrideStore Store, FallbackAccessBadgeOverrideValidator Validator, IClock Clock, List<string> Journal, FallbackAccessBadgeOverrideEngine Engine)
{
    public static FallbackAccessBadgeOverrideWorkflow NewFallbackAccessBadgeOverrideWorkflow(IFallbackAccessBadgeOverrideStore store, IClock clock)
    {
        return new FallbackAccessBadgeOverrideWorkflow(
            store,
            FallbackAccessBadgeOverrideRules.DefaultFallbackAccessBadgeOverrideValidator(),
            clock,
            new List<string>(),
            FallbackAccessBadgeOverrideEngine.DefaultFallbackAccessBadgeOverrideEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback access badge override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackAccessBadgeOverrideStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback access badge override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackAccessBadgeOverrideStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback access badge override {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.ExpiresAt > cutoff)
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
