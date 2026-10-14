namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy succession plan policies through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LegacySuccessionPlanPolicyDesk(ILegacySuccessionPlanPolicyStore Store, LegacySuccessionPlanPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy succession plan policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacySuccessionPlanPolicyStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy succession plan policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacySuccessionPlanPolicyStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of legacy succession plan policy workflow.</summary>
public static class LegacySuccessionPlanPolicyWorkflow
{
    public static LegacySuccessionPlanPolicyDesk NewLegacySuccessionPlanPolicyDesk(ILegacySuccessionPlanPolicyStore store, IClock clock)
    {
        return new LegacySuccessionPlanPolicyDesk(
            store,
            LegacySuccessionPlanPolicyRules.DefaultLegacySuccessionPlanPolicyValidator(),
            clock,
            new List<string>());
    }
}
