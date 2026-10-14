namespace Acme.Workforce.Engagement.PulseSurvey.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual pulse survey quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualPulseSurveyQuotaService(IManualPulseSurveyQuotaStore Store, ManualPulseSurveyQuotaValidator Validator, IClock Clock, List<string> Journal, ManualPulseSurveyQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual pulse survey quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualPulseSurveyQuotaStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual pulse survey quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualPulseSurveyQuotaStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual pulse survey quota {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of manual pulse survey quota workflow.</summary>
public static class ManualPulseSurveyQuotaWorkflow
{
    public static ManualPulseSurveyQuotaService NewManualPulseSurveyQuotaService(IManualPulseSurveyQuotaStore store, IClock clock, Registry flags)
    {
        return new ManualPulseSurveyQuotaService(
            store,
            ManualPulseSurveyQuotaRules.DefaultManualPulseSurveyQuotaValidator(),
            clock,
            new List<string>(),
            ManualPulseSurveyQuotaEngine.DefaultManualPulseSurveyQuotaEngine(flags));
    }
}
