namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive career path exceptions through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveCareerPathExceptionCoordinator(IAdaptiveCareerPathExceptionStore Store, AdaptiveCareerPathExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive career path exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveCareerPathExceptionStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive career path exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveCareerPathExceptionStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of adaptive career path exception workflow.</summary>
public static class AdaptiveCareerPathExceptionWorkflow
{
    public static AdaptiveCareerPathExceptionCoordinator NewAdaptiveCareerPathExceptionCoordinator(IAdaptiveCareerPathExceptionStore store, IClock clock)
    {
        return new AdaptiveCareerPathExceptionCoordinator(
            store,
            AdaptiveCareerPathExceptionRules.DefaultAdaptiveCareerPathExceptionValidator(),
            clock,
            new List<string>());
    }
}
