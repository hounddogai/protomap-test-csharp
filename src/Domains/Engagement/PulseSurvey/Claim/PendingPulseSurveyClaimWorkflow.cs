namespace Acme.Workforce.Engagement.PulseSurvey.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending pulse survey claims through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingPulseSurveyClaimDesk(IPendingPulseSurveyClaimStore Store, PendingPulseSurveyClaimValidator Validator, IClock Clock, List<string> Journal, PendingPulseSurveyClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pulse survey claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingPulseSurveyClaimStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pulse survey claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingPulseSurveyClaimStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pulse survey claim {code} not found");
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

/// <summary>Functions of pending pulse survey claim workflow.</summary>
public static class PendingPulseSurveyClaimWorkflow
{
    public static PendingPulseSurveyClaimDesk NewPendingPulseSurveyClaimDesk(IPendingPulseSurveyClaimStore store, IClock clock, Registry flags)
    {
        return new PendingPulseSurveyClaimDesk(
            store,
            PendingPulseSurveyClaimRules.DefaultPendingPulseSurveyClaimValidator(),
            clock,
            new List<string>(),
            PendingPulseSurveyClaimEngine.DefaultPendingPulseSurveyClaimEngine(flags));
    }
}
