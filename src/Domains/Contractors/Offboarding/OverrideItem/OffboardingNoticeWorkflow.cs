namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves offboarding notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingNoticeWorkflow(IOffboardingNoticeStore Store, OffboardingNoticeValidator Validator, IClock Clock, List<string> Journal, OffboardingNoticeEngine Engine)
{
    public static OffboardingNoticeWorkflow NewOffboardingNoticeWorkflow(IOffboardingNoticeStore store, IClock clock)
    {
        return new OffboardingNoticeWorkflow(
            store,
            OffboardingNoticeRules.DefaultOffboardingNoticeValidator(),
            clock,
            new List<string>(),
            OffboardingNoticeEngine.DefaultOffboardingNoticeEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingNoticeStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingNoticeStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding notice {code} not found");
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
