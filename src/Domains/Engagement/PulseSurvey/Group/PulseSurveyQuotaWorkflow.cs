namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyQuotaDesk(IPulseSurveyQuotaStore Store, PulseSurveyQuotaValidator Validator, IClock Clock, List<string> Journal, PulseSurveyQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyQuotaStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyQuotaStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey quota {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of pulse survey quota workflow.</summary>
public static class PulseSurveyQuotaWorkflow
{
    public static PulseSurveyQuotaDesk NewPulseSurveyQuotaDesk(IPulseSurveyQuotaStore store, IClock clock)
    {
        return new PulseSurveyQuotaDesk(
            store,
            PulseSurveyQuotaRules.DefaultPulseSurveyQuotaValidator(),
            clock,
            new List<string>(),
            PulseSurveyQuotaEngine.DefaultPulseSurveyQuotaEngine());
    }
}
