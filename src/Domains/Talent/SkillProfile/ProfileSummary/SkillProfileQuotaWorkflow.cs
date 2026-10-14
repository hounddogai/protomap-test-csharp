namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves skill profile quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileQuotaWorkflow(ISkillProfileQuotaStore Store, SkillProfileQuotaValidator Validator, IClock Clock, List<string> Journal, SkillProfileQuotaEngine Engine)
{
    public static SkillProfileQuotaWorkflow NewSkillProfileQuotaWorkflow(ISkillProfileQuotaStore store, IClock clock)
    {
        return new SkillProfileQuotaWorkflow(
            store,
            SkillProfileQuotaRules.DefaultSkillProfileQuotaValidator(),
            clock,
            new List<string>(),
            SkillProfileQuotaEngine.DefaultSkillProfileQuotaEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileQuotaStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileQuotaStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile quota {code} not found");
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
