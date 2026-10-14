namespace Acme.Workforce.Orgdesign.Position.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered position bundle is in its life cycle.</summary>
public enum TieredPositionBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tiered position bundle. Its code is unique within a tenant.</summary>
public sealed class TieredPositionBundle(string code, string name, string segment, string employeeId, double minimum, TieredPositionBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public string EmployeeId { get; set; } = employeeId;
    public double Minimum { get; set; } = minimum;
    public TieredPositionBundleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered position bundle in status draft.</summary>
    public static TieredPositionBundle NewTieredPositionBundle(string code, string name)
    {
        return new TieredPositionBundle(code, name, "", "", 0.0, TieredPositionBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPositionBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPositionBundleStatus next)
    {
        switch (Status)
        {
            case TieredPositionBundleStatus.Draft:
                return next == TieredPositionBundleStatus.Active;
            case TieredPositionBundleStatus.Pending:
                return next == TieredPositionBundleStatus.Closed;
            case TieredPositionBundleStatus.Active:
                switch (next)
                {
                    case TieredPositionBundleStatus.Suspended:
                    case TieredPositionBundleStatus.Closed:
                        return true;
                }
                return false;
            case TieredPositionBundleStatus.Suspended:
                switch (next)
                {
                    case TieredPositionBundleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredPositionBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered position bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Minimum;
    }

    public TieredPositionBundle Copy()
    {
        return new TieredPositionBundle(Code, Name, Segment, EmployeeId, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
