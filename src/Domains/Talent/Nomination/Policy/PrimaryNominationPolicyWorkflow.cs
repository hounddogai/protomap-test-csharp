namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary nomination policies through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryNominationPolicyCoordinator(IPrimaryNominationPolicyStore Store, PrimaryNominationPolicyValidator Validator, IClock Clock, List<string> Journal, PrimaryNominationPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary nomination policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryNominationPolicyStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary nomination policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryNominationPolicyStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary nomination policy {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of primary nomination policy workflow.</summary>
public static class PrimaryNominationPolicyWorkflow
{
    public static PrimaryNominationPolicyCoordinator NewPrimaryNominationPolicyCoordinator(IPrimaryNominationPolicyStore store, IClock clock)
    {
        return new PrimaryNominationPolicyCoordinator(
            store,
            PrimaryNominationPolicyRules.DefaultPrimaryNominationPolicyValidator(),
            clock,
            new List<string>(),
            PrimaryNominationPolicyEngine.DefaultPrimaryNominationPolicyEngine());
    }
}
