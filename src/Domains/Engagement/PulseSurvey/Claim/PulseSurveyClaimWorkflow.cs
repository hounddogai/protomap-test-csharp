namespace Acme.Workforce.Engagement.PulseSurvey.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyClaimService(IPulseSurveyClaimStore Store, PulseSurveyClaimValidator Validator, IClock Clock, List<string> Journal, PulseSurveyClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyClaimStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyClaimStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey claim {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of pulse survey claim workflow.</summary>
public static class PulseSurveyClaimWorkflow
{
    public static PulseSurveyClaimService NewPulseSurveyClaimService(IPulseSurveyClaimStore store, IClock clock)
    {
        return new PulseSurveyClaimService(
            store,
            PulseSurveyClaimRules.DefaultPulseSurveyClaimValidator(),
            clock,
            new List<string>(),
            PulseSurveyClaimEngine.DefaultPulseSurveyClaimEngine());
    }
}
