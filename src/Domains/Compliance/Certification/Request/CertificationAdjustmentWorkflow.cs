namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves certification adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationAdjustmentWorkflow(ICertificationAdjustmentStore Store, CertificationAdjustmentValidator Validator, IClock Clock, List<string> Journal, CertificationAdjustmentEngine Engine)
{
    public static CertificationAdjustmentWorkflow NewCertificationAdjustmentWorkflow(ICertificationAdjustmentStore store, IClock clock)
    {
        return new CertificationAdjustmentWorkflow(
            store,
            CertificationAdjustmentRules.DefaultCertificationAdjustmentValidator(),
            clock,
            new List<string>(),
            CertificationAdjustmentEngine.DefaultCertificationAdjustmentEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationAdjustmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationAdjustmentStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification adjustment {code} not found");
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
