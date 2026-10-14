namespace Acme.Workforce.Orgdesign.Position.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position estimate is in its life cycle.</summary>
public enum PositionEstimateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One position estimate, with what the planners need to act on it.</summary>
public sealed class PositionEstimate(string code, string name, int capacity, double minimum, string employeeId, PositionEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public double Minimum { get; set; } = minimum;
    public string EmployeeId { get; set; } = employeeId;
    public PositionEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a position estimate in status created.</summary>
    public static PositionEstimate NewPositionEstimate(string code, string name)
    {
        return new PositionEstimate(code, name, 0, 0.0, "", PositionEstimateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionEstimateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionEstimateStatus next)
    {
        switch (Status)
        {
            case PositionEstimateStatus.Created:
                return next == PositionEstimateStatus.Validated || next == PositionEstimateStatus.Published;
            case PositionEstimateStatus.Validated:
                switch (next)
                {
                    case PositionEstimateStatus.Published:
                    case PositionEstimateStatus.Retired:
                        return true;
                }
                return false;
            case PositionEstimateStatus.Published:
                switch (next)
                {
                    case PositionEstimateStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Minimum + (double)Capacity, 2);
    }

    public PositionEstimate Copy()
    {
        return new PositionEstimate(Code, Name, Capacity, Minimum, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
