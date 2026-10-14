namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves mobility rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record MobilityRuleWorkflow(IMobilityRuleStore Store, MobilityRuleValidator Validator, IClock Clock, List<string> Journal)
{
    public static MobilityRuleWorkflow NewMobilityRuleWorkflow(IMobilityRuleStore store, IClock clock)
    {
        return new MobilityRuleWorkflow(
            store,
            MobilityRuleRules.DefaultMobilityRuleValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MobilityRuleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MobilityRuleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
