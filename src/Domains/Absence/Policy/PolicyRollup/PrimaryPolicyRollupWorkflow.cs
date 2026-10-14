namespace Acme.Workforce.Absence.Policy.PolicyRollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary policy rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryPolicyRollupWorkflow(IPrimaryPolicyRollupStore Store, PrimaryPolicyRollupValidator Validator, IClock Clock, List<string> Journal, PrimaryPolicyRollupEngine Engine)
{
    public static PrimaryPolicyRollupWorkflow NewPrimaryPolicyRollupWorkflow(IPrimaryPolicyRollupStore store, IClock clock, Registry flags)
    {
        return new PrimaryPolicyRollupWorkflow(
            store,
            PrimaryPolicyRollupRules.DefaultPrimaryPolicyRollupValidator(),
            clock,
            new List<string>(),
            PrimaryPolicyRollupEngine.DefaultPrimaryPolicyRollupEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary policy rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryPolicyRollupStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary policy rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryPolicyRollupStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary policy rollup {code} not found");
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
