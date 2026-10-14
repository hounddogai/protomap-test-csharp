namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual skill profile policies through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualSkillProfilePolicyManager(IManualSkillProfilePolicyStore Store, ManualSkillProfilePolicyValidator Validator, IClock Clock, List<string> Journal, ManualSkillProfilePolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual skill profile policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualSkillProfilePolicyStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual skill profile policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualSkillProfilePolicyStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual skill profile policy {code} not found");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of manual skill profile policy workflow.</summary>
public static class ManualSkillProfilePolicyWorkflow
{
    public static ManualSkillProfilePolicyManager NewManualSkillProfilePolicyManager(IManualSkillProfilePolicyStore store, IClock clock)
    {
        return new ManualSkillProfilePolicyManager(
            store,
            ManualSkillProfilePolicyRules.DefaultManualSkillProfilePolicyValidator(),
            clock,
            new List<string>(),
            ManualSkillProfilePolicyEngine.DefaultManualSkillProfilePolicyEngine());
    }
}
