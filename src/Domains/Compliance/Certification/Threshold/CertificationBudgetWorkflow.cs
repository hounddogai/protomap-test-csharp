namespace Acme.Workforce.Compliance.Certification.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves certification budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationBudgetManager(ICertificationBudgetStore Store, CertificationBudgetValidator Validator, IClock Clock, List<string> Journal, CertificationBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationBudgetStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationBudgetStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification budget {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of certification budget workflow.</summary>
public static class CertificationBudgetWorkflow
{
    public static CertificationBudgetManager NewCertificationBudgetManager(ICertificationBudgetStore store, IClock clock, Registry flags)
    {
        return new CertificationBudgetManager(
            store,
            CertificationBudgetRules.DefaultCertificationBudgetValidator(),
            clock,
            new List<string>(),
            CertificationBudgetEngine.DefaultCertificationBudgetEngine(flags));
    }
}
