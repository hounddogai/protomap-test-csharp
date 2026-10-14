namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback exception assignment is in its life cycle.</summary>
public enum FallbackExceptionAssignmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a fallback exception assignment and its current status.</summary>
public sealed class FallbackExceptionAssignment(string code, string name, double fte, int version, double cost, FallbackExceptionAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public int Version { get; set; } = version;
    public double Cost { get; set; } = cost;
    public FallbackExceptionAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback exception assignment in status created.</summary>
    public static FallbackExceptionAssignment NewFallbackExceptionAssignment(string code, string name)
    {
        return new FallbackExceptionAssignment(code, name, 0.0, 0, 0.0, FallbackExceptionAssignmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackExceptionAssignmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackExceptionAssignmentStatus next)
    {
        switch (Status)
        {
            case FallbackExceptionAssignmentStatus.Created:
                return next == FallbackExceptionAssignmentStatus.Validated || next == FallbackExceptionAssignmentStatus.Published;
            case FallbackExceptionAssignmentStatus.Validated:
                switch (next)
                {
                    case FallbackExceptionAssignmentStatus.Published:
                        return true;
                }
                return false;
            case FallbackExceptionAssignmentStatus.Published:
                switch (next)
                {
                    case FallbackExceptionAssignmentStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackExceptionAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback exception assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Fte + Cost, 2);
    }

    public FallbackExceptionAssignment Copy()
    {
        return new FallbackExceptionAssignment(Code, Name, Fte, Version, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
