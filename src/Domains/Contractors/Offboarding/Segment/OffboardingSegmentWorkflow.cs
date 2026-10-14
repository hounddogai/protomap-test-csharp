namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves offboarding segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingSegmentWorkflow(IOffboardingSegmentStore Store, OffboardingSegmentValidator Validator, IClock Clock, List<string> Journal, OffboardingSegmentEngine Engine)
{
    public static OffboardingSegmentWorkflow NewOffboardingSegmentWorkflow(IOffboardingSegmentStore store, IClock clock, Registry flags)
    {
        return new OffboardingSegmentWorkflow(
            store,
            OffboardingSegmentRules.DefaultOffboardingSegmentValidator(),
            clock,
            new List<string>(),
            OffboardingSegmentEngine.DefaultOffboardingSegmentEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingSegmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingSegmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding segment {code} not found");
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
