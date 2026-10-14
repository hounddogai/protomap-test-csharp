namespace Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model digest is in its life cycle.</summary>
public enum CostModelDigestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a cost model digest and its current status.</summary>
public sealed class CostModelDigest(string code, string label, string description, double amount, int limit, string name, string externalRef, string skill, string jobCode, double discount, CostModelDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public double Amount { get; set; } = amount;
    public int Limit { get; set; } = limit;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public string Skill { get; set; } = skill;
    public string JobCode { get; set; } = jobCode;
    public double Discount { get; set; } = discount;
    public CostModelDigestStatus Status { get; set; } = status;

    /// <summary>Starts a cost model digest in status open.</summary>
    public static CostModelDigest NewCostModelDigest(string code, string label)
    {
        return new CostModelDigest(code, label, "", 0.0, 0, "", "", "", "", 0.0, CostModelDigestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelDigestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelDigestStatus next)
    {
        switch (Status)
        {
            case CostModelDigestStatus.Open:
                switch (next)
                {
                    case CostModelDigestStatus.Resolved:
                    case CostModelDigestStatus.Archived:
                        return true;
                }
                return false;
            case CostModelDigestStatus.InReview:
                switch (next)
                {
                    case CostModelDigestStatus.Reopened:
                    case CostModelDigestStatus.Archived:
                        return true;
                }
                return false;
            case CostModelDigestStatus.Resolved:
                return next == CostModelDigestStatus.Reopened || next == CostModelDigestStatus.Archived;
            case CostModelDigestStatus.Reopened:
                switch (next)
                {
                    case CostModelDigestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetTotal()
    {
        return Math.Round(Amount + Discount, 2);
    }

    public CostModelDigest Copy()
    {
        return new CostModelDigest(
            Code,
            Label,
            Description,
            Amount,
            Limit,
            Name,
            ExternalRef,
            Skill,
            JobCode,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
