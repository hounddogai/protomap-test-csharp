namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive offboarding segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveOffboardingSegmentWorkflow(IAdaptiveOffboardingSegmentStore Store, AdaptiveOffboardingSegmentValidator Validator, IClock Clock, List<string> Journal, AdaptiveOffboardingSegmentEngine Engine)
{
    public static AdaptiveOffboardingSegmentWorkflow NewAdaptiveOffboardingSegmentWorkflow(IAdaptiveOffboardingSegmentStore store, IClock clock, Registry flags)
    {
        return new AdaptiveOffboardingSegmentWorkflow(
            store,
            AdaptiveOffboardingSegmentRules.DefaultAdaptiveOffboardingSegmentValidator(),
            clock,
            new List<string>(),
            AdaptiveOffboardingSegmentEngine.DefaultAdaptiveOffboardingSegmentEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive offboarding segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveOffboardingSegmentStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive offboarding segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveOffboardingSegmentStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive offboarding segment {code} not found");
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
