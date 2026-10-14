namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict deduction rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictDeductionRuleWorkflow(IStrictDeductionRuleStore Store, StrictDeductionRuleValidator Validator, IClock Clock, List<string> Journal, StrictDeductionRuleEngine Engine)
{
    public static StrictDeductionRuleWorkflow NewStrictDeductionRuleWorkflow(IStrictDeductionRuleStore store, IClock clock)
    {
        return new StrictDeductionRuleWorkflow(
            store,
            StrictDeductionRuleRules.DefaultStrictDeductionRuleValidator(),
            clock,
            new List<string>(),
            StrictDeductionRuleEngine.DefaultStrictDeductionRuleEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict deduction rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictDeductionRuleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict deduction rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictDeductionRuleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict deduction rule {code} not found");
        }
        var result = Engine.Run(item);
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
