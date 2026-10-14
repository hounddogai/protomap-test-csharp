namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive career path notices through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveCareerPathNoticeManager(IAdaptiveCareerPathNoticeStore Store, AdaptiveCareerPathNoticeValidator Validator, IClock Clock, List<string> Journal, AdaptiveCareerPathNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive career path notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveCareerPathNoticeStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive career path notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveCareerPathNoticeStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive career path notice {code} not found");
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

/// <summary>Functions of adaptive career path notice workflow.</summary>
public static class AdaptiveCareerPathNoticeWorkflow
{
    public static AdaptiveCareerPathNoticeManager NewAdaptiveCareerPathNoticeManager(IAdaptiveCareerPathNoticeStore store, IClock clock)
    {
        return new AdaptiveCareerPathNoticeManager(
            store,
            AdaptiveCareerPathNoticeRules.DefaultAdaptiveCareerPathNoticeValidator(),
            clock,
            new List<string>(),
            AdaptiveCareerPathNoticeEngine.DefaultAdaptiveCareerPathNoticeEngine());
    }
}
