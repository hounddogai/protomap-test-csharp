namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model variance is in its life cycle.</summary>
public enum CostModelVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A cost model variance. Its code is unique within a tenant.</summary>
public sealed class CostModelVariance(string code, string name, double minimum, string skill, string unit, CostModelVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public string Skill { get; set; } = skill;
    public string Unit { get; set; } = unit;
    public CostModelVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a cost model variance in status draft.</summary>
    public static CostModelVariance NewCostModelVariance(string code, string name)
    {
        return new CostModelVariance(code, name, 0.0, "", "", CostModelVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelVarianceStatus next)
    {
        switch (Status)
        {
            case CostModelVarianceStatus.Draft:
                switch (next)
                {
                    case CostModelVarianceStatus.Pending:
                    case CostModelVarianceStatus.Suspended:
                        return true;
                }
                return false;
            case CostModelVarianceStatus.Pending:
                switch (next)
                {
                    case CostModelVarianceStatus.Suspended:
                        return true;
                }
                return false;
            case CostModelVarianceStatus.Active:
                switch (next)
                {
                    case CostModelVarianceStatus.Suspended:
                    case CostModelVarianceStatus.Closed:
                        return true;
                }
                return false;
            case CostModelVarianceStatus.Suspended:
                return next == CostModelVarianceStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CostModelVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Minimum;
    }

    public CostModelVariance Copy()
    {
        return new CostModelVariance(Code, Name, Minimum, Skill, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
