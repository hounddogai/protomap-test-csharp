namespace Acme.Workforce.Payrollops.PayRun.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay run agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayRunAgreementWorkflow(IPayRunAgreementStore Store, PayRunAgreementValidator Validator, IClock Clock, List<string> Journal, PayRunAgreementEngine Engine)
{
    public static PayRunAgreementWorkflow NewPayRunAgreementWorkflow(IPayRunAgreementStore store, IClock clock)
    {
        return new PayRunAgreementWorkflow(
            store,
            PayRunAgreementRules.DefaultPayRunAgreementValidator(),
            clock,
            new List<string>(),
            PayRunAgreementEngine.DefaultPayRunAgreementEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayRunAgreementStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayRunAgreementStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run agreement {code} not found");
        }
        var result = Engine.Evaluate(item);
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
